using System;
using Dapper;
using VSurvServer.Core.Sessions;
using VSurvServer.Infrastructure.Database;
using VSurvServer.Infrastructure.Logging;
using VSurvServer.Protocol.Packets;

namespace VSurvServer.Core.Services;

public class EquipService
{
    public EquipResponse Handle(EquipRequest request, ClientSession session)
    {
        if (session.LoggedInUserId <= 0)
        {
            return new EquipResponse { Success = false, Message = "로그인이 필요합니다." };
        }

        try
        {
            using (var db = DatabaseManager.GetConnection())
            {
                db.Open();

                // 1. 보안 체크: 유저가 정말로 그 무기를 가지고 있는지 확인 (핵 방지)
                string checkSql = "SELECT COUNT(*) FROM user_inventory WHERE user_id = @UserId AND weapon_id = @WeaponId";
                int count = db.ExecuteScalar<int>(checkSql, new { UserId = session.LoggedInUserId, WeaponId = request.WeaponId });

                if (count == 0)
                {
                    return new EquipResponse { Success = false, Message = "보유하지 않은 무기입니다." };
                }

                // 2. 장착 처리: users 테이블의 equipped_weapon_id 업데이트
                string updateSql = "UPDATE users SET equipped_weapon_id = @WeaponId WHERE id = @UserId";
                db.Execute(updateSql, new { WeaponId = request.WeaponId, UserId = session.LoggedInUserId });

                ServerLogger.Info($"[무기 장착] 유저({session.LoggedInUsername})가 무기(ID: {request.WeaponId})를 장착했습니다.");

                return new EquipResponse
                {
                    Success = true,
                    EquippedWeaponId = request.WeaponId,
                    Message = "무기를 장착했습니다!"
                };
            }
        }
        catch (Exception ex)
        {
            ServerLogger.Error($"[장착 오류] {ex.Message}");
            return new EquipResponse { Success = false, Message = "무기 장착 중 오류가 발생했습니다." };
        }
    }
}