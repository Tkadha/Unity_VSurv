using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum PacketId : byte
{
    None = 0,
    StartGameRequest = 1,
    StartGameResponse = 2,
}

[Serializable]
public class StartGameRequest
{
    public string PlayerName;
}

[Serializable]
public class StartGameResponse
{
    public bool Success;
    public string Message;
}

public class GameServerClient : MonoBehaviour
{
    private const int HeaderSize = 3;

    [Header("Server")]
    [SerializeField] private string serverIp = "127.0.0.1";
    [SerializeField] private int serverPort = 7777;

    public async Task<StartGameResponse> RequestStartGameAsync(string playerName)
    {
        try
        {
            using TcpClient client = new TcpClient();
            await client.ConnectAsync(serverIp, serverPort);

            using NetworkStream stream = client.GetStream();

            StartGameRequest request = new StartGameRequest
            {
                PlayerName = playerName
            };

            string requestJson = JsonUtility.ToJson(request);
            await SendPacketAsync(stream, PacketId.StartGameRequest, requestJson);

            PacketReadResult? result = await ReceivePacketAsync(stream);
            if (result == null)
            {
                return new StartGameResponse
                {
                    Success = false,
                    Message = "서버 응답이 없습니다."
                };
            }

            if (result.PacketId != PacketId.StartGameResponse)
            {
                return new StartGameResponse
                {
                    Success = false,
                    Message = "예상하지 못한 응답 패킷입니다."
                };
            }

            Debug.Log($"[GameServerClient] Response PacketId: {result.PacketId}, Payload: {result.PayloadJson}");

            StartGameResponse response = JsonUtility.FromJson<StartGameResponse>(result.PayloadJson);

            if (response == null)
            {
                return new StartGameResponse
                {
                    Success = false,
                    Message = "응답 파싱에 실패했습니다."
                };
            }

            return response;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameServerClient] RequestStartGameAsync 예외: {ex.Message}");

            return new StartGameResponse
            {
                Success = false,
                Message = "서버 연결에 실패했습니다."
            };
        }
    }

    private async Task SendPacketAsync(NetworkStream stream, PacketId packetId, string payloadJson)
    {
        byte[] payloadBuffer = Encoding.UTF8.GetBytes(payloadJson);
        ushort totalSize = checked((ushort)(HeaderSize + payloadBuffer.Length));

        byte[] packetBuffer = new byte[totalSize];

        Array.Copy(BitConverter.GetBytes(totalSize), 0, packetBuffer, 0, 2);
        packetBuffer[2] = (byte)packetId;
        Array.Copy(payloadBuffer, 0, packetBuffer, HeaderSize, payloadBuffer.Length);

        await stream.WriteAsync(packetBuffer, 0, packetBuffer.Length);
    }

    private async Task<PacketReadResult?> ReceivePacketAsync(NetworkStream stream)
    {
        byte[] headerBuffer = await ReadExactAsync(stream, HeaderSize);
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
            ? await ReadExactAsync(stream, payloadSize)
            : Array.Empty<byte>();

        if (payloadSize > 0 && payloadBuffer.Length == 0)
        {
            return null;
        }

        string payloadJson = Encoding.UTF8.GetString(payloadBuffer);

        return new PacketReadResult((PacketId)packetIdValue, payloadJson);
    }

    private async Task<byte[]> ReadExactAsync(NetworkStream stream, int length)
    {
        byte[] buffer = new byte[length];
        int totalRead = 0;

        while (totalRead < length)
        {
            int bytesRead = await stream.ReadAsync(buffer, totalRead, length - totalRead);

            if (bytesRead <= 0)
            {
                return Array.Empty<byte>();
            }

            totalRead += bytesRead;
        }

        return buffer;
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
}