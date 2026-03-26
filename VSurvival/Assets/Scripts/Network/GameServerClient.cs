using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class GameServerClient : MonoBehaviour
{
    private const int HeaderSize = 3;

    [Header("Server")]
    [SerializeField] private string serverIp = "127.0.0.1";
    [SerializeField] private int serverPort = 7777;


    [SerializeField] private float pingIntervalSeconds = 5f;

    private CancellationTokenSource _pingLoopCts;
    private Task _pingLoopTask;

    private TcpClient _tcpClient;
    private NetworkStream _stream;
    private CancellationTokenSource _receiveLoopCts;
    private Task _receiveLoopTask;

    private TaskCompletionSource<StartGameResponse> _pendingStartGameResponse;
    private TaskCompletionSource<PingResponse> _pendingPingResponse;
    private TaskCompletionSource<EndGameResponse> _pendingEndGameResponse;
    private TaskCompletionSource<RegisterResponse> _pendingRegisterResponse;
    private TaskCompletionSource<LoginResponse> _pendingLoginResponse;
    private TaskCompletionSource<RankingResponse> _pendingRankingResponse;

    public bool IsConnected => _tcpClient != null && _tcpClient.Connected;

    public async Task<bool> ConnectAsync()
    {
        if (IsConnected)
        {
            Debug.Log("[GameServerClient] 이미 서버에 연결된 상태입니다.");
            return true;
        }

        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(serverIp, serverPort);

            _stream = _tcpClient.GetStream();
            _receiveLoopCts = new CancellationTokenSource();
            _receiveLoopTask = RunReceiveLoopAsync(_receiveLoopCts.Token);

            _pingLoopCts = new CancellationTokenSource();
            _pingLoopTask = RunPingLoopAsync(_pingLoopCts.Token);

            Debug.Log($"[GameServerClient] 서버 연결 성공 - {serverIp}:{serverPort}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameServerClient] ConnectAsync 예외: {ex.Message}");
            Disconnect();
            return false;
        }
    }

    public void Disconnect()
    {
        try
        {
            _receiveLoopCts?.Cancel();
        }
        catch
        {
        }
        try
        {
            _pingLoopCts?.Cancel();
        }
        catch
        {
        }
        try
        {
            _stream?.Close();
        }
        catch
        {
        }
        try
        {
            _tcpClient?.Close();
        }
        catch
        {
        }

        _stream = null;
        _tcpClient = null;
        _receiveLoopCts = null;
        _receiveLoopTask = null;

        FailPendingRequests("서버 연결이 종료되었습니다.");

        Debug.Log("[GameServerClient] 서버 연결 종료");
    }

    private void HandleReceivedPacket(PacketReadResult result)
    {
        Debug.Log($"[GameServerClient] 수신 패킷 - PacketId: {result.PacketId}, Payload: {result.PayloadJson}");

        switch (result.PacketId)
        {
            case PacketId.StartGameResponse:
                {
                    StartGameResponse response = JsonUtility.FromJson<StartGameResponse>(result.PayloadJson);

                    if (response == null)
                    {
                        response = new StartGameResponse
                        {
                            Success = false,
                            Message = "StartGame 응답 파싱에 실패했습니다."
                        };
                    }

                    _pendingStartGameResponse?.TrySetResult(response);
                    _pendingStartGameResponse = null;
                    break;
                }

            case PacketId.EndGameResponse:
                {
                    EndGameResponse response = JsonUtility.FromJson<EndGameResponse>(result.PayloadJson);

                    if (response == null)
                    {
                        response = new EndGameResponse { Success = false };
                    }

                    _pendingEndGameResponse?.TrySetResult(response);
                    _pendingEndGameResponse = null;
                    break;
                }

            case PacketId.PingResponse:
                {
                    PingResponse response = JsonUtility.FromJson<PingResponse>(result.PayloadJson);

                    if (response == null)
                    {
                        response = new PingResponse
                        {
                            Success = false,
                            Message = "Ping 응답 파싱에 실패했습니다.",
                            ClientTimeTicks = 0,
                            ServerTimeTicks = 0
                        };
                    }

                    _pendingPingResponse?.TrySetResult(response);
                    _pendingPingResponse = null;
                    break;
                }

            case PacketId.RegisterResponse:
                {
                    RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(result.PayloadJson);

                    if (response == null)
                    {
                        response = new RegisterResponse { Success = false, Message = "응답 파싱 실패" };
                    }

                    _pendingRegisterResponse?.TrySetResult(response);
                    _pendingRegisterResponse = null;
                    break;
                }

            case PacketId.LoginResponse:
                {
                    LoginResponse response = JsonUtility.FromJson<LoginResponse>(result.PayloadJson);
                    if (response == null) response = new LoginResponse { Success = false, Message = "응답 파싱 실패" };

                    _pendingLoginResponse?.TrySetResult(response);
                    _pendingLoginResponse = null;
                    break;
                }

            case PacketId.RankingResponse:
                {
                    RankingResponse response = JsonUtility.FromJson<RankingResponse>(result.PayloadJson);

                    if (response == null)
                    {
                        response = new RankingResponse { Success = false, TopRanks = new RankEntry[0] };
                    }

                    _pendingRankingResponse?.TrySetResult(response);
                    _pendingRankingResponse = null;
                    break;
                }
            default:
                {
                    Debug.LogWarning($"[GameServerClient] 처리되지 않은 PacketId 수신: {result.PacketId}");
                    break;
                }
        }
    }


    public async Task<StartGameResponse> RequestStartGameAsync(string playerName)
    {
        if (!IsConnected)
        {
            bool connected = await ConnectAsync();
            if (!connected)
            {
                return new StartGameResponse
                {
                    Success = false,
                    Message = "서버 연결에 실패했습니다."
                };
            }
        }

        if (_pendingStartGameResponse != null && !_pendingStartGameResponse.Task.IsCompleted)
        {
            return new StartGameResponse
            {
                Success = false,
                Message = "이전 StartGame 요청이 아직 처리 중입니다."
            };
        }

        _pendingStartGameResponse = new TaskCompletionSource<StartGameResponse>();

        StartGameRequest request = new StartGameRequest
        {
            PlayerName = playerName
        };

        string requestJson = JsonUtility.ToJson(request);
        await SendPacketAsync(PacketId.StartGameRequest, requestJson);

        return await _pendingStartGameResponse.Task;
    }
    public async Task<EndGameResponse> RequestEndGameAsync(int score)
    {
        if (!IsConnected) return new EndGameResponse { Success = false };

        if (_pendingEndGameResponse != null && !_pendingEndGameResponse.Task.IsCompleted)
        {
            return new EndGameResponse { Success = false };
        }

        _pendingEndGameResponse = new TaskCompletionSource<EndGameResponse>();

        EndGameRequest request = new EndGameRequest { Score = score };
        string requestJson = JsonUtility.ToJson(request);

        await SendPacketAsync(PacketId.EndGameRequest, requestJson);
        return await _pendingEndGameResponse.Task;
    }
    public async Task<PingResponse> RequestPingAsync()
    {
        if (!IsConnected)
        {
            bool connected = await ConnectAsync();
            if (!connected)
            {
                return new PingResponse
                {
                    Success = false,
                    Message = "서버 연결에 실패했습니다.",
                    ClientTimeTicks = 0,
                    ServerTimeTicks = 0
                };
            }
        }

        return await RequestPingInternalAsync();
    }
    public async Task<RegisterResponse> RequestRegisterAsync(string username, string password)
    {
        if (!IsConnected)
        {
            bool connected = await ConnectAsync();
            if (!connected)
            {
                return new RegisterResponse { Success = false, Message = "서버 연결에 실패했습니다." };
            }
        }

        if (_pendingRegisterResponse != null && !_pendingRegisterResponse.Task.IsCompleted)
        {
            return new RegisterResponse { Success = false, Message = "이전 요청이 처리 중입니다." };
        }

        _pendingRegisterResponse = new TaskCompletionSource<RegisterResponse>();

        RegisterRequest request = new RegisterRequest
        {
            Username = username,
            Password = password
        };

        string requestJson = JsonUtility.ToJson(request);
        await SendPacketAsync(PacketId.RegisterRequest, requestJson);

        return await _pendingRegisterResponse.Task;
    }
    public async Task<LoginResponse> RequestLoginAsync(string username, string password)
    {
        if (!IsConnected)
        {
            bool connected = await ConnectAsync();
            if (!connected) return new LoginResponse { Success = false, Message = "서버 연결에 실패했습니다." };
        }

        if (_pendingLoginResponse != null && !_pendingLoginResponse.Task.IsCompleted)
        {
            return new LoginResponse { Success = false, Message = "이전 요청이 처리 중입니다." };
        }

        _pendingLoginResponse = new TaskCompletionSource<LoginResponse>();

        LoginRequest request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        string requestJson = JsonUtility.ToJson(request);
        await SendPacketAsync(PacketId.LoginRequest, requestJson);

        return await _pendingLoginResponse.Task;
    }
    public async Task<RankingResponse> RequestRankingAsync()
    {
        if (!IsConnected)
        {
            bool connected = await ConnectAsync();
            if (!connected) return new RankingResponse { Success = false };
        }

        if (_pendingRankingResponse != null && !_pendingRankingResponse.Task.IsCompleted)
        {
            return new RankingResponse { Success = false };
        }

        _pendingRankingResponse = new TaskCompletionSource<RankingResponse>();

        RankingRequest request = new RankingRequest();
        string requestJson = JsonUtility.ToJson(request);

        await SendPacketAsync(PacketId.RankingRequest, requestJson);

        return await _pendingRankingResponse.Task;
    }
    private async Task<PingResponse> RequestPingInternalAsync()
    {
        if (!IsConnected)
        {
            return new PingResponse
            {
                Success = false,
                Message = "서버에 연결되어 있지 않습니다.",
                ClientTimeTicks = 0,
                ServerTimeTicks = 0
            };
        }

        if (_pendingPingResponse != null && !_pendingPingResponse.Task.IsCompleted)
        {
            return new PingResponse
            {
                Success = false,
                Message = "이전 Ping 요청이 아직 처리 중입니다.",
                ClientTimeTicks = 0,
                ServerTimeTicks = 0
            };
        }

        _pendingPingResponse = new TaskCompletionSource<PingResponse>();

        PingRequest request = new PingRequest
        {
            ClientTimeTicks = DateTime.UtcNow.Ticks
        };

        string requestJson = JsonUtility.ToJson(request);
        await SendPacketAsync(PacketId.PingRequest, requestJson);

        return await _pendingPingResponse.Task;
    }
    private async Task RunReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                PacketReadResult result = await ReceivePacketAsync(cancellationToken);

                if (result == null)
                {
                    Debug.LogWarning("[GameServerClient] 서버로부터 패킷을 더 이상 수신하지 못했습니다.");
                    break;
                }

                HandleReceivedPacket(result);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameServerClient] RunReceiveLoopAsync 예외: {ex.Message}");
        }
        finally
        {
            Disconnect();
        }
    }
    private async Task RunPingLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                await Task.Delay(TimeSpan.FromSeconds(pingIntervalSeconds), cancellationToken);

                if (cancellationToken.IsCancellationRequested || !IsConnected)
                {
                    break;
                }

                PingResponse response = await RequestPingInternalAsync();

                if (!response.Success)
                {
                    Debug.LogWarning($"[GameServerClient] 자동 Ping 실패: {response.Message}");
                }
                else
                {
                    Debug.Log($"[GameServerClient] 자동 Ping 성공 - ClientTimeTicks: {response.ClientTimeTicks}, ServerTimeTicks: {response.ServerTimeTicks}");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameServerClient] RunPingLoopAsync 예외: {ex.Message}");
        }
    }
    private async Task SendPacketAsync(PacketId packetId, string payloadJson)
    {
        if (!IsConnected || _stream == null)
        {
            throw new InvalidOperationException("서버에 연결되어 있지 않습니다.");
        }

        byte[] payloadBuffer = Encoding.UTF8.GetBytes(payloadJson);
        ushort totalSize = checked((ushort)(HeaderSize + payloadBuffer.Length));

        byte[] packetBuffer = new byte[totalSize];

        Array.Copy(BitConverter.GetBytes(totalSize), 0, packetBuffer, 0, 2);
        packetBuffer[2] = (byte)packetId;
        Array.Copy(payloadBuffer, 0, packetBuffer, HeaderSize, payloadBuffer.Length);

        await _stream.WriteAsync(packetBuffer, 0, packetBuffer.Length);
    }

    private async Task<PacketReadResult> ReceivePacketAsync(CancellationToken cancellationToken)
    {
        if (!IsConnected || _stream == null)
        {
            return null;
        }

        byte[] headerBuffer = await ReadExactAsync(HeaderSize, cancellationToken);
        if (headerBuffer.Length == 0)
        {
            return null;
        }

        ushort totalSize = BitConverter.ToUInt16(headerBuffer, 0);
        byte packetIdValue = headerBuffer[2];

        if (totalSize < HeaderSize)
        {
            return null;
        }

        int payloadSize = totalSize - HeaderSize;
        byte[] payloadBuffer = payloadSize > 0
            ? await ReadExactAsync(payloadSize, cancellationToken)
            : Array.Empty<byte>();

        if (payloadSize > 0 && payloadBuffer.Length == 0)
        {
            return null;
        }

        string payloadJson = Encoding.UTF8.GetString(payloadBuffer);
        return new PacketReadResult((PacketId)packetIdValue, payloadJson);
    }

    private async Task<byte[]> ReadExactAsync(int length, CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[length];
        int totalRead = 0;

        while (totalRead < length)
        {
            int bytesRead = await _stream.ReadAsync(buffer, totalRead, length - totalRead, cancellationToken);

            if (bytesRead <= 0)
            {
                return Array.Empty<byte>();
            }

            totalRead += bytesRead;
        }

        return buffer;
    }

    private void FailPendingRequests(string message)
    {
        if (_pendingStartGameResponse != null && !_pendingStartGameResponse.Task.IsCompleted)
        {
            _pendingStartGameResponse.TrySetResult(new StartGameResponse
            {
                Success = false,
                Message = message
            });
        }

        _pendingStartGameResponse = null;

        if (_pendingPingResponse != null && !_pendingPingResponse.Task.IsCompleted)
        {
            _pendingPingResponse.TrySetResult(new PingResponse
            {
                Success = false,
                Message = message,
                ClientTimeTicks = 0,
                ServerTimeTicks = 0
            });
        }

        _pendingPingResponse = null;
    }

    private sealed class PacketReadResult
    {
        public PacketId PacketId { get; }
        public string PayloadJson { get; }

        public PacketReadResult(PacketId packetId, string payloadJson)
        {
            PacketId = packetId;
            PayloadJson = payloadJson;
        }
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}