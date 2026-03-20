using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using VSurvServer.Core.Game;
using VSurvServer.Core.Services;
using VSurvServer.Core.Sessions;
using VSurvServer.Infrastructure.Logging;
using VSurvServer.Protocol.Packets;

namespace VSurvServer.Server.Hosting;

public class ServerHost
{
    private TcpListener? _listener;
    private bool _isRunning;

    private readonly StartGameService _startGameService = new();
    private ServerGameState _currentState = ServerGameState.Lobby;

    private readonly ConcurrentDictionary<int, ClientSession> _sessions = new();
    private int _nextSessionId = 0;

    private readonly object _stateLock = new();

    public void Start()
    {
        if (_isRunning)
        {
            return;
        }

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
                            lock (_stateLock)
                            {
                                response = _startGameService.Handle(request, _currentState);

                                if (response.Success)
                                {
                                    _currentState = ServerGameState.Playing;
                                }
                            }
                        }

                        string responseJson = JsonSerializer.Serialize(response);
                        await session.SendPacketAsync(PacketId.StartGameResponse, responseJson);

                        ServerLogger.Info($"응답 데이터 전송 - SessionId: {session.SessionId}, PacketId: {PacketId.StartGameResponse}, Payload: {responseJson}");
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