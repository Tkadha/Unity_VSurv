using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using VSurvServer.Core.Game;
using VSurvServer.Core.Services;
using VSurvServer.Infrastructure.Logging;
using VSurvServer.Protocol.Packets;

namespace VSurvServer.Server.Hosting;

public class ServerHost
{
    private TcpListener? _listener;
    private bool _isRunning;
    private readonly StartGameService _startGameService = new();
    private ServerGameState _currentState = ServerGameState.Lobby;

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

        Task.Run(AcceptLoop);
    }

    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        _isRunning = false;
        _listener?.Stop();

        ServerLogger.Info("서버 종료 처리");
    }

    private async Task AcceptLoop()
    {
        if (_listener == null)
        {
            return;
        }

        while (_isRunning)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync();
                ServerLogger.Info("클라이언트 접속 수락");

                _ = Task.Run(() => HandleClientAsync(client));
            }
            catch (Exception ex)
            {
                if (_isRunning)
                {
                    ServerLogger.Error($"AcceptLoop 예외 발생: {ex.Message}");
                }
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        {
            try
            {
                using NetworkStream stream = client.GetStream();

                byte[] buffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead <= 0)
                {
                    ServerLogger.Error("수신 데이터 없음");
                    return;
                }

                string requestJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                ServerLogger.Info($"수신 데이터: {requestJson}");

                StartGameRequest? request = JsonSerializer.Deserialize<StartGameRequest>(requestJson);
                if (request == null)
                {
                    var invalidResponse = new StartGameResponse
                    {
                        Success = false,
                        Message = "잘못된 요청입니다."
                    };

                    await SendResponseAsync(stream, invalidResponse);
                    return;
                }

                StartGameResponse response = _startGameService.Handle(request, _currentState);

                if (response.Success)
                {
                    _currentState = ServerGameState.Playing;
                }

                await SendResponseAsync(stream, response);
            }
            catch (Exception ex)
            {
                ServerLogger.Error($"HandleClientAsync 예외 발생: {ex.Message}");
            }
        }
    }

    private async Task SendResponseAsync(NetworkStream stream, StartGameResponse response)
    {
        string responseJson = JsonSerializer.Serialize(response);
        byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson);

        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        ServerLogger.Info($"응답 데이터 전송: {responseJson}");
    }
}