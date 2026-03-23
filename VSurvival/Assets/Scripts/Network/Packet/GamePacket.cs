using System;

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

[Serializable]
public class PingRequest
{
    public long ClientTimeTicks;
}

[Serializable]
public class PingResponse
{
    public bool Success;
    public long ClientTimeTicks;
    public long ServerTimeTicks;
    public string Message;
}

[Serializable]
public class EndGameRequest
{
    public PacketId PacketId => PacketId.EndGameRequest;
}

[Serializable]
public class EndGameResponse
{
    public PacketId PacketId => PacketId.EndGameResponse;
    public bool Success { get; set; }
}