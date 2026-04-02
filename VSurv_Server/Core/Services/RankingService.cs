using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data;
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
            var redisDb = RedisManager.GetDatabase();
            string rankingKey = "GlobalRanking";

            // 명령어: ZREVRANGE GlobalRanking 0 9 WITHSCORES (내림차순으로 1위~10위 추출)
            var topScores = redisDb.SortedSetRangeByRankWithScores(
                key: rankingKey,
                start: 0,
                stop: 9,
                order: Order.Descending
            );

            // 3. 클라이언트(유니티)가 이해할 수 있는 배열 형태로 변환
            List<RankEntry> rankList = new List<RankEntry>();
            int currentRank = 1;

            foreach (var entry in topScores)
            {
                rankList.Add(new RankEntry
                {
                    Rank = currentRank++,
                    // Redis에서 꺼낸 멤버 이름은 RedisValue 타입이므로 string으로 변환
                    Username = entry.Element.ToString(),
                    // 점수는 double 타입으로 반환되므로 int로 형변환
                    Score = (int)entry.Score
                });
            }

            // 4. 성공 응답 반환
            return new RankingResponse
            {
                Success = true,
                TopRanks = rankList.ToArray()
            };
        }
        catch (Exception ex)
        {
            ServerLogger.Error($"[Redis] 랭킹 조회 중 오류: {ex.Message}");

            return new RankingResponse { Success = false, TopRanks = Array.Empty<RankEntry>() };
        }
    }

    public static void WarmUpCache()
    {
        ServerLogger.Info("Redis 랭킹 캐시 웜업(Warm-up)을 시작합니다...");
        try
        {
            using (IDbConnection db = DatabaseManager.GetConnection())
            {
                db.Open();

                // 1. MySQL에서 상위 100명의 데이터를 가져옵니다.
                string sql = @"
                    SELECT u.username AS Username, s.highest_score AS Score 
                    FROM user_scores s
                    INNER JOIN users u ON s.user_id = u.id
                    ORDER BY s.highest_score DESC
                    LIMIT 100";

                var topScores = db.Query<RankEntry>(sql).ToList();

                if (topScores.Count > 0)
                {
                    var redisDb = RedisManager.GetDatabase();
                    string rankingKey = "GlobalRanking";

                    // 2. 혹시 남아있을지 모를 쓰레기 데이터를 비워줍니다.
                    redisDb.KeyDelete(rankingKey);

                    // foreach로 한 건씩 넣으면 통신 비용이 크므로, SortedSetEntry 배열로 만들어 한 번에 밀어 넣습니다.
                    var redisEntries = new SortedSetEntry[topScores.Count];
                    for (int i = 0; i < topScores.Count; i++)
                    {
                        // Username을 RedisValue로, Score를 double로 매핑
                        redisEntries[i] = new SortedSetEntry(topScores[i].Username, topScores[i].Score);
                    }

                    // 한 번의 네트워크 요청으로 100건 일괄 삽입
                    redisDb.SortedSetAdd(rankingKey, redisEntries);

                    ServerLogger.Info($"캐시 웜업 완료: {topScores.Count}명의 랭킹 데이터를 Redis에 적재했습니다.");
                }
                else
                {
                    ServerLogger.Info("캐시 웜업: DB에 적재할 랭킹 데이터가 없습니다.");
                }
            }
        }
        catch (Exception ex)
        {
            ServerLogger.Error($"캐시 웜업 중 오류 발생: {ex.Message}");
        }
    }
}