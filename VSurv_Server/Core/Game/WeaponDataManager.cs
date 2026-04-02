using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using VSurvServer.Infrastructure.Database;
using VSurvServer.Infrastructure.Logging;

namespace VSurvServer.Core.Game;

public static class WeaponDataManager
{
    // 무기 ID를 키(Key)로, 무기 정보를 값(Value)으로 가지는 초고속 메모리 저장소
    private static readonly Dictionary<int, WeaponDefinition> _weapons = new();

    // 서버 구동 시 단 한 번 호출됨
    public static void LoadAllWeapons()
    {
        ServerLogger.Info("무기 도감(Master Data) 로딩을 시작합니다...");

        try
        {
            using (IDbConnection db = DatabaseManager.GetConnection())
            {
                db.Open();

                // Dapper가 C# 프로퍼티와 정확히 매핑할 수 있도록 AS 키워드 사용
                string sql = @"
                    SELECT 
                        id AS Id, 
                        name AS Name, 
                        power AS Power, 
                        fire_rate AS FireRate, 
                        rarity AS Rarity, 
                        prefab_name AS PrefabName, 
                        ammo_type AS AmmoType, 
                        description AS Description 
                    FROM weapon_definitions";

                var weaponList = db.Query<WeaponDefinition>(sql).ToList();

                _weapons.Clear();
                foreach (var weapon in weaponList)
                {
                    _weapons.Add(weapon.Id, weapon);
                }

                ServerLogger.Info($"무기 도감 로딩 완료: 총 {_weapons.Count}종의 무기가 메모리에 적재되었습니다.");
            }
        }
        catch (Exception ex)
        {
            ServerLogger.Error($"무기 도감 로딩 중 치명적 오류: {ex.Message}");
            throw; // 마스터 데이터가 없으면 게임 진행이 불가능하므로 서버를 강제로 멈춥니다.
        }
    }

    // 게임 로직에서 무기 스탯이 필요할 때 호출하는 메서드 (O(1) 속도)
    public static WeaponDefinition? GetWeapon(int id)
    {
        if (_weapons.TryGetValue(id, out var weapon))
        {
            return weapon;
        }
        return null; // 없는 무기 번호를 요청했을 때
    }
}