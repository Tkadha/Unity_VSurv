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