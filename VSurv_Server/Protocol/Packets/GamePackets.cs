namespace VSurvServer.Protocol.Packets;

public class StartGameRequest
{
    public PacketId PacketId => PacketId.StartGameRequest;

    public string PlayerName { get; set; } = string.Empty;
}

public class StartGameResponse
{
    public PacketId PacketId => PacketId.StartGameResponse;

    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
public class PingRequest
{
    public PacketId PacketId => PacketId.PingRequest;

    public long ClientTimeTicks { get; set; }
}

public class PingResponse
{
    public PacketId PacketId => PacketId.PingResponse;

    public bool Success { get; set; }
    public long ClientTimeTicks { get; set; }
    public long ServerTimeTicks { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class EndGameRequest
{
    public PacketId PacketId => PacketId.EndGameRequest;
}

public class EndGameResponse
{
    public PacketId PacketId => PacketId.EndGameResponse;
    public bool Success { get; set; }
}
