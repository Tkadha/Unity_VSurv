using VSurvServer.Core.Game;
using VSurvServer.Protocol.Packets;

namespace VSurvServer.Core.Services;

public class StartGameService
{
    public StartGameResponse Handle(StartGameRequest request, ServerGameState currentState)
    {
        if (currentState != ServerGameState.Lobby)
        {
            return new StartGameResponse
            {
                Success = false,
                Message = "게임 시작이 가능한 상태가 아닙니다."
            };
        }

        return new StartGameResponse
        {
            Success = true,
            Message = "게임 시작 요청이 승인되었습니다."
        };
    }
}