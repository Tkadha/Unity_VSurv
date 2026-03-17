namespace VSurvServer.Core.Sessions;

public class ClientSession
{
    public int SessionId { get; }
    public bool IsConnected { get; private set; }

    public ClientSession(int sessionId)
    {
        SessionId = sessionId;
        IsConnected = true;
    }

    public void Disconnect()
    {
        IsConnected = false;
    }
}