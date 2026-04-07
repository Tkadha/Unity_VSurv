using System;
using System.Linq;
using Dapper;
using VSurvServer.Core.Sessions;
using VSurvServer.Infrastructure.Database;
using VSurvServer.Infrastructure.Logging;
using VSurvServer.Protocol.Packets;

namespace VSurvServer.Core.Services;

public class InventoryService
{
    public InventoryResponse Handle(InventoryRequest request, ClientSession session)
    {
        if (session.LoggedInUserId <= 0)
        {
            return new InventoryResponse { Success = false, Message = "로그인이 필요합니다." };
        }

        try
        {
            using (var db = DatabaseManager.GetConnection())
            {
                db.Open();

                // JOIN을 사용하여 유저가 가진 무기의 ID와 이름을 한 번에 가져옵니다.
                string sql = @"
                    SELECT 
                        w.id AS WeaponId, 
                        w.name AS WeaponName 
                    FROM user_inventory ui
                    JOIN weapon_definitions w ON ui.weapon_id = w.id
                    WHERE ui.user_id = @UserId";

                var items = db.Query<InventoryItem>(sql, new { UserId = session.LoggedInUserId }).ToList();

                return new InventoryResponse
                {
                    Success = true,
                    Items = items,
                    Message = "인벤토리 조회 성공"
                };
            }
        }
        catch (Exception ex)
        {
            ServerLogger.Error($"[인벤토리 오류] {ex.Message}");
            return new InventoryResponse { Success = false, Message = "인벤토리를 불러오는 중 오류가 발생했습니다." };
        }
    }
}