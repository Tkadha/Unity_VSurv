using System.Net.Sockets;
using System.Text;

namespace VSurvServer.Core.Sessions;

public class ClientSession
{
    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _stream;
    private bool _isDisconnected;

    public int SessionId { get; }
    public bool IsConnected => !_isDisconnected && _tcpClient.Connected;

    public ClientSession(int sessionId, TcpClient tcpClient)
    {
        SessionId = sessionId;
        _tcpClient = tcpClient;
        _stream = tcpClient.GetStream();
    }

    public async Task RunAsync(
        Func<ClientSession, string, Task> onMessageReceived,
        Func<ClientSession, Task>? onDisconnected = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                string? message = await ReceiveOnceAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(message))
                {
                    break;
                }

                await onMessageReceived(this, message);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            Disconnect();

            if (onDisconnected != null)
            {
                await onDisconnected(this);
            }
        }
    }

    public async Task<string?> ReceiveOnceAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            return null;
        }

        byte[] buffer = new byte[4096];

        int bytesRead = await _stream.ReadAsync(buffer, cancellationToken);
        if (bytesRead <= 0)
        {
            return null;
        }

        return Encoding.UTF8.GetString(buffer, 0, bytesRead);
    }

    public async Task SendAsync(string message, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(message);
        await _stream.WriteAsync(data, cancellationToken);
    }

    public void Disconnect()
    {
        if (_isDisconnected)
        {
            return;
        }

        _isDisconnected = true;

        try
        {
            _stream.Close();
        }
        catch
        {
        }

        try
        {
            _tcpClient.Close();
        }
        catch
        {
        }
    }
}