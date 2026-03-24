using System.Net.Sockets;
using System.Text;
using VSurvServer.Core.Game;
using VSurvServer.Protocol.Packets;

namespace VSurvServer.Core.Sessions;

public class ClientSession
{
    private const int HeaderSize = 3;

    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _stream;
    private bool _isDisconnected;

    public int SessionId { get; }
    public bool IsConnected => !_isDisconnected && _tcpClient.Connected;
    public GameRoom? CurrentRoom { get; set; }
    public int LoggedInUserId { get; set; } = 0;

    public ClientSession(int sessionId, TcpClient tcpClient)
    {
        SessionId = sessionId;
        _tcpClient = tcpClient;
        _stream = tcpClient.GetStream();
    }

    public async Task RunAsync(
        Func<ClientSession, PacketId, string, Task> onPacketReceived,
        Func<ClientSession, Task>? onDisconnected = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                PacketReadResult? result = await ReceivePacketAsync(cancellationToken);

                if (result == null)
                {
                    break;
                }

                await onPacketReceived(this, result.PacketId, result.PayloadJson);
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

    public async Task<PacketReadResult?> ReceivePacketAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
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

    public async Task SendPacketAsync(PacketId packetId, string payloadJson, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            return;
        }

        byte[] payloadBuffer = Encoding.UTF8.GetBytes(payloadJson);
        ushort totalSize = checked((ushort)(HeaderSize + payloadBuffer.Length));

        byte[] packetBuffer = new byte[totalSize];

        Array.Copy(BitConverter.GetBytes(totalSize), 0, packetBuffer, 0, 2);
        packetBuffer[2] = (byte)packetId;
        Array.Copy(payloadBuffer, 0, packetBuffer, HeaderSize, payloadBuffer.Length);

        await _stream.WriteAsync(packetBuffer, cancellationToken);
    }

    private async Task<byte[]> ReadExactAsync(int length, CancellationToken cancellationToken = default)
    {
        byte[] buffer = new byte[length];
        int totalRead = 0;

        while (totalRead < length)
        {
            int bytesRead = await _stream.ReadAsync(
                buffer.AsMemory(totalRead, length - totalRead),
                cancellationToken);

            if (bytesRead <= 0)
            {
                return Array.Empty<byte>();
            }

            totalRead += bytesRead;
        }

        return buffer;
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

public sealed class PacketReadResult
{
    public PacketId PacketId { get; }
    public string PayloadJson { get; }

    public PacketReadResult(PacketId packetId, string payloadJson)
    {
        PacketId = packetId;
        PayloadJson = payloadJson;
    }
}