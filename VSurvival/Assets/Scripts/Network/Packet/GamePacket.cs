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