namespace VSurvServer.Core.Game;

public class GameRoom
{
    public int RoomId { get; }
    public ServerGameState State { get; private set; } = ServerGameState.Lobby;

    public GameRoom(int roomId)
    {
        RoomId = roomId;
    }

    public bool TryStartGame()
    {
        if (State != ServerGameState.Lobby) return false;

        State = ServerGameState.Playing;
        return true;
    }

    public void EndGame()
    {
        State = ServerGameState.Lobby;
    }
}