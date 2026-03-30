using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using VSurvServer.Core.Game;
using VSurvServer.Core.Services;
using VSurvServer.Core.Sessions;
using VSurvServer.Infrastructure.Database;
using VSurvServer.Infrastructure.Logging;
using VSurvServer.Protocol.Packets;

namespace VSurvServer.Server.Hosting;

public class ServerHost
{
    private TcpListener? _listener;
    private bool _isRunning;

    private readonly RegisterService _registerService = new();
    private readonly LoginService _loginService = new();
    private readonly EndGameService _endGameService = new();
    private readonly RankingService _rankingService = new();

    private readonly ConcurrentDictionary<int, ClientSession> _sessions = new();
    private int _nextSessionId = 0;


    public void Start()
    {
        if (_isRunning)
        {
            return;
        }

        ServerLogger.Info("데이터베이스 연결 확인 중...");
        if (!DatabaseManager.TestConnection())
        {
            ServerLogger.Error("데이터베이스 연결에 실패하여 서버 구동을 중단합니다.");
            return;
        }
        ServerLogger.Info("데이터베이스 연결 성공!");

        ServerLogger.Info("Redis 캐시 서버 연결 확인 중...");
        if (!RedisManager.Initialize())
        {
            ServerLogger.Error("Redis 연결 실패로 서버 구동을 중단합니다. redis-server가 켜져 있는지 확인하세요.");
            return;
        }
        ServerLogger.Info("Redis 연결 성공!");

        _listener = new TcpListener(IPAddress.Any, 7777);
        _listener.Start();
        _isRunning = true;

        ServerLogger.Info("서버 초기화 완료");
        ServerLogger.Info("TCP 리스너 시작 - Port: 7777");

        Task.Run(AcceptLoopAsync);
    }

    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        _isRunning = false;
        _listener?.Stop();

        foreach (var sessionPair in _sessions)
        {
            sessionPair.Value.Disconnect();
        }

        _sessions.Clear();

        ServerLogger.Info("서버 종료 처리");
    }

    private async Task AcceptLoopAsync()
    {
        if (_listener == null)
        {
            return;
        }

        while (_isRunning)
        {
            try
            {
                TcpClient tcpClient = await _listener.AcceptTcpClientAsync();
                int sessionId = Interlocked.Increment(ref _nextSessionId);
                ClientSession session = new ClientSession(sessionId, tcpClient);
                session.CurrentRoom = new GameRoom(sessionId);

                if (_sessions.TryAdd(sessionId, session))
                {
                    ServerLogger.Info($"클라이언트 접속 수락 - SessionId: {sessionId}, ConnectedSessions: {_sessions.Count}");

                    _ = Task.Run(() => session.RunAsync(
                                        OnSessionPacketReceivedAsync,
                                        OnSessionDisconnectedAsync));
                }
                else
                {
                    ServerLogger.Error($"세션 등록 실패 - SessionId: {sessionId}");
                    session.Disconnect();
                }
            }
            catch (Exception ex)
            {
                if (_isRunning)
                {
                    ServerLogger.Error($"AcceptLoopAsync 예외 발생: {ex.Message}");
                }
            }
        }
    }

    private async Task OnSessionPacketReceivedAsync(ClientSession session, PacketId packetId, string payloadJson)
    {
        try
        {
            ServerLogger.Info($"수신 데이터 - SessionId: {session.SessionId}, PacketId: {packetId}, Payload: {payloadJson}");

            switch (packetId)
            {
                case PacketId.StartGameRequest:
                    {
                        StartGameRequest? request = JsonSerializer.Deserialize<StartGameRequest>(payloadJson);

                        StartGameResponse response;
                        if (request == null)
                        {
                            response = new StartGameResponse
                            {
                                Success = false,
                                Message = "잘못된 요청입니다."
                            };
                        }
                        else
                        {
                            bool success = session.CurrentRoom?.TryStartGame() ?? false;

                            response = new StartGameResponse
                            {
                                Success = success,
                                Message = success ? "게임 시작 승인" : "시작 불가 상태"
                            };
                        }

                        string responseJson = JsonSerializer.Serialize(response);
                        await session.SendPacketAsync(PacketId.StartGameResponse, responseJson);

                        ServerLogger.Info($"응답 데이터 전송 - SessionId: {session.SessionId}, PacketId: {PacketId.StartGameResponse}, Payload: {responseJson}");
                        break;
                    }

                case PacketId.EndGameRequest:
                    {
                        EndGameRequest? request = JsonSerializer.Deserialize<EndGameRequest>(payloadJson);
                        EndGameResponse response;

                        if (request == null)
                        {
                            response = new EndGameResponse { Success = false };
                        }
                        else
                        {
                            // 💡 세션 정보와 방 정보를 서비스로 넘김
                            response = _endGameService.Handle(request, session, session.CurrentRoom);
                        }

                        string responseJson = JsonSerializer.Serialize(response);
                        await session.SendPacketAsync(PacketId.EndGameResponse, responseJson);
                        break;
                    }

                case PacketId.PingRequest:
                    {
                        PingRequest? request = JsonSerializer.Deserialize<PingRequest>(payloadJson);

                        PingResponse response;
                        if (request == null)
                        {
                            response = new PingResponse
                            {
                                Success = false,
                                Message = "잘못된 Ping 요청입니다.",
                                ClientTimeTicks = 0,
                                ServerTimeTicks = DateTime.UtcNow.Ticks
                            };
                        }
                        else
                        {
                            response = new PingResponse
                            {
                                Success = true,
                                Message = "Pong",
                                ClientTimeTicks = request.ClientTimeTicks,
                                ServerTimeTicks = DateTime.UtcNow.Ticks
                            };
                        }

                        string responseJson = JsonSerializer.Serialize(response);
                        await session.SendPacketAsync(PacketId.PingResponse, responseJson);

                        ServerLogger.Info($"응답 데이터 전송 - SessionId: {session.SessionId}, PacketId: {PacketId.PingResponse}, Payload: {responseJson}");
                        break;
                    }

                case PacketId.RegisterRequest:
                    {
                        RegisterRequest? request = JsonSerializer.Deserialize<RegisterRequest>(payloadJson);
                        RegisterResponse response;

                        if (request == null)
                        {
                            response = new RegisterResponse { Success = false, Message = "잘못된 요청 데이터입니다." };
                        }
                        else
                        {
                            response = _registerService.Handle(request);
                        }


                        string responseJson = JsonSerializer.Serialize(response);
                        await session.SendPacketAsync(PacketId.RegisterResponse, responseJson);

                        ServerLogger.Info($"회원가입 시도 - Username: {request?.Username}, 결과: {response.Success}");
                        break;
                    }

                case PacketId.LoginRequest:
                    {
                        LoginRequest? request = JsonSerializer.Deserialize<LoginRequest>(payloadJson);
                        LoginResponse response;

                        if (request == null)
                        {
                            response = new LoginResponse { Success = false, Message = "잘못된 요청입니다." };
                        }
                        else
                        {
                            response = _loginService.Handle(request);

                            if (response.Success)
                            {
                                session.LoggedInUserId = response.UserId;
                                session.LoggedInUsername = request.Username;
                                ServerLogger.Info($"유저 로그인 성공 - Username: {request.Username}, UserId: {response.UserId}");
                            }
                        }

                        string responseJson = JsonSerializer.Serialize(response);
                        await session.SendPacketAsync(PacketId.LoginResponse, responseJson);
                        break;
                    }

                case PacketId.RankingRequest:
                    {
                        RankingRequest? request = JsonSerializer.Deserialize<RankingRequest>(payloadJson);
                        RankingResponse response;

                        if (request == null)
                        {
                            response = new RankingResponse { Success = false, TopRanks = Array.Empty<RankEntry>() };
                        }
                        else
                        {
                            response = _rankingService.Handle(request);
                        }

                        string responseJson = JsonSerializer.Serialize(response);
                        await session.SendPacketAsync(PacketId.RankingResponse, responseJson);
                        break;
                    }

                default:
                    {
                        ServerLogger.Error($"알 수 없는 PacketId 수신 - SessionId: {session.SessionId}, PacketId: {packetId}");
                        break;
                    }
            }
        }
        catch (Exception ex)
        {
            ServerLogger.Error($"OnSessionPacketReceivedAsync 예외 발생 - SessionId: {session.SessionId}, Message: {ex.Message}");
            session.Disconnect();
        }
    }

    private Task OnSessionDisconnectedAsync(ClientSession session)
    {
        RemoveSession(session.SessionId);
        return Task.CompletedTask;
    }

    private void RemoveSession(int sessionId)
    {
        if (_sessions.TryRemove(sessionId, out ClientSession? session))
        {
            session.Disconnect();
            ServerLogger.Info($"세션 종료 - SessionId: {sessionId}, ConnectedSessions: {_sessions.Count}");
        }
    }
}