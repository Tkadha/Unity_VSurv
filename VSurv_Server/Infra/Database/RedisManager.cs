using System;
using StackExchange.Redis;
using VSurvServer.Infrastructure.Logging;

namespace VSurvServer.Infrastructure.Database; 

public static class RedisManager
{
    private static readonly string ConnectionString = "localhost:6379";

    private static ConnectionMultiplexer? _redis;

    public static bool Initialize()
    {
        try
        {
            _redis = ConnectionMultiplexer.Connect(ConnectionString);

            if (_redis.IsConnected)
            {
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            ServerLogger.Error($"Redis 연결 초기화 실패: {ex.Message}");
            return false;
        }
    }

    public static IDatabase GetDatabase()
    {
        if (_redis == null || !_redis.IsConnected)
        {
            throw new InvalidOperationException("Redis가 초기화되지 않았거나 연결이 끊어졌습니다.");
        }
        return _redis.GetDatabase();
    }
}