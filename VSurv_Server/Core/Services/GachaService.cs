using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using VSurvServer.Core.Game;
using VSurvServer.Core.Sessions;
using VSurvServer.Infrastructure.Database;
using VSurvServer.Infrastructure.Logging;
using VSurvServer.Protocol.Packets;

namespace VSurvServer.Core.Services;

public class GachaService
{
    private static readonly Random _random = new Random();

    public GachaResponse Handle(GachaRequest request, ClientSession session)
    {
        // 1. 로그인 확인
        if (session.LoggedInUserId <= 0)
        {
            return new GachaResponse { Success = false, Message = "로그인이 필요합니다." };
        }

        try
        {
            var allWeapons = WeaponDataManager.GetAllWeapons();

            if (allWeapons.Count == 0)
            {
                ServerLogger.Error("가챠 실패: 서버에 로드된 무기 데이터가 없습니다.");
                return new GachaResponse { Success = false, Message = "뽑을 수 있는 무기가 없습니다." };
            }

            int index = _random.Next(allWeapons.Count);
            WeaponDefinition selectedWeapon = allWeapons[index];

            using (IDbConnection db = DatabaseManager.GetConnection())
            {
                db.Open();
                string sql = @"
                    INSERT INTO user_inventory (user_id, weapon_id) 
                    VALUES (@UserId, @WeaponId)";

                db.Execute(sql, new { UserId = session.LoggedInUserId, WeaponId = selectedWeapon.Id });
            }

            ServerLogger.Info($"[가챠 성공] 유저({session.LoggedInUsername})가 '{selectedWeapon.Name}' 획득!");

            return new GachaResponse
            {
                Success = true,
                WeaponId = selectedWeapon.Id,
                WeaponName = selectedWeapon.Name,
                Rarity = selectedWeapon.Rarity,
                Message = "뽑기 성공!"
            };
        }
        catch (Exception ex)
        {
            ServerLogger.Error($"[가챠 오류] {ex.Message}");
            return new GachaResponse { Success = false, Message = "서버 오류로 뽑기에 실패했습니다." };
        }
    }
}