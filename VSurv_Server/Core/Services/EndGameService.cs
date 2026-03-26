using System;
using System.Data;
using Dapper;
using VSurvServer.Core.Game;
using VSurvServer.Core.Sessions;
using VSurvServer.Infrastructure.Database;
using VSurvServer.Infrastructure.Logging;
using VSurvServer.Protocol.Packets;

namespace VSurvServer.Core.Services;

public class EndGameService
{
    public EndGameResponse Handle(EndGameRequest request, ClientSession session, GameRoom? room)
    {
        room?.EndGame();

        if (session.LoggedInUserId <= 0)
        {
            ServerLogger.Info($"비로그인 유저 게임 종료 - Score: {request.Score}");
            return new EndGameResponse { Success = true };
        }

        try
        {
            using (IDbConnection db = DatabaseManager.GetConnection())
            {
                db.Open();

                string sql = @"
                    INSERT INTO user_scores (user_id, highest_score) 
                    VALUES (@UserId, @Score) 
                    ON DUPLICATE KEY UPDATE highest_score = GREATEST(highest_score, @Score)";

                db.Execute(sql, new { UserId = session.LoggedInUserId, Score = request.Score });

                ServerLogger.Info($"점수 갱신 처리 완료 - UserId: {session.LoggedInUserId}, Score: {request.Score}");
            }
        }
        catch (Exception ex)
        {
            ServerLogger.Error($"점수 저장 중 DB 오류 발생: {ex.Message}");
        }

        return new EndGameResponse { Success = true };
    }
}