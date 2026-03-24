using MySqlConnector;
using System.Data;
using VSurvServer.Infrastructure.Logging;

namespace VSurvServer.Infrastructure.Database;

public static class DatabaseManager
{
    
    private static readonly string ConnectionString = "Server=localhost;Port=3306;Database=vsurv_db;Uid=root;Pwd=8923;";

    public static IDbConnection GetConnection()
    {
        return new MySqlConnection(ConnectionString);
    }

    public static bool TestConnection()
    {
        try
        {
            using (IDbConnection db = GetConnection())
            {
                db.Open();

                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = "SELECT 1";
                    cmd.ExecuteScalar();
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            // 연결 실패 시 로그를 남김
            ServerLogger.Error($"DB 연결 테스트 실패: {ex.Message}");
            return false;
        }
    }

}