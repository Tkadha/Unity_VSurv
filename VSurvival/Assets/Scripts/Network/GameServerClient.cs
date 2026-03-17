using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
            byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);

            await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

            byte[] buffer = new byte[4096];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead <= 0)
            {
                return new StartGameResponse
                {
                    Success = false,
                    Message = "서버 응답이 없습니다."
                };
            }

            string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Debug.Log($"[GameServerClient] Response: {responseJson}");

            StartGameResponse response = JsonUtility.FromJson<StartGameResponse>(responseJson);

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
}