using System;
using System.Collections.Generic;
using StackExchange.Redis;
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
}