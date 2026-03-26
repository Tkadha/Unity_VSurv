using System;
using System.Data;
using System.Linq;
using Dapper;
using VSurvServer.Infrastructure.Database;
using VSurvServer.Infrastructure.Logging;
using VSurvServer.Protocol.Packets;

namespace VSurvServer.Core.Services;

public class RankingService
{
    public RankingResponse Handle(RankingRequest request)
    {
        try
        {
            using (IDbConnection db = DatabaseManager.GetConnection())
            {
                db.Open();

                string sql = @"
                    SELECT 
                        u.username AS Username, 
                        s.highest_score AS Score 
                    FROM user_scores s
                    INNER JOIN users u ON s.user_id = u.id
                    ORDER BY s.highest_score DESC, s.updated_at ASC
                    LIMIT 10";

                var rawRanks = db.Query<RankEntry>(sql).ToList();

                for (int i = 0; i < rawRanks.Count; i++)
                {
                    rawRanks[i].Rank = i + 1;
                }

                return new RankingResponse
                {
                    Success = true,
                    TopRanks = rawRanks.ToArray()
                };
            }
        }
        catch (Exception ex)
        {
            ServerLogger.Error($"랭킹 조회 중 DB 오류: {ex.Message}");
            return new RankingResponse { Success = false, TopRanks = Array.Empty<RankEntry>() };
        }
    }
}