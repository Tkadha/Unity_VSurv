using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    // 어디서든 쉽게 접근할 수 있게 Instance(싱글톤)로 만듭니다.
    public static DataManager Instance;

    // 무기 ID를 넣으면 무기 스탯을 꺼내주는 사전(Dictionary)
    public Dictionary<int, WeaponStatData> WeaponTable = new Dictionary<int, WeaponStatData>();

    private void Awake()
    {
        Instance = this;
        InitWeaponData();
    }

    private void InitWeaponData()
    {
        // 💡 나중에는 엑셀(CSV) 등에서 불러오지만, 지금은 테스트를 위해 직접 입력합니다.
        WeaponTable.Add(1, new WeaponStatData { WeaponId = 1, WeaponName = "Pistol", AttackPower = 5f, FireInterval = 0.4f });
        WeaponTable.Add(2, new WeaponStatData { WeaponId = 2, WeaponName = "SMG", AttackPower = 3f, FireInterval = 0.1f });
        WeaponTable.Add(3, new WeaponStatData { WeaponId = 3, WeaponName = "AR", AttackPower = 12f, FireInterval = 0.25f });
        WeaponTable.Add(4, new WeaponStatData { WeaponId = 4, WeaponName = "Rifle", AttackPower = 50f, FireInterval = 1.5f });
    }

    // 무기 ID로 스탯을 찾아주는 함수
    public WeaponStatData GetWeaponStat(int weaponId)
    {
        if (WeaponTable.ContainsKey(weaponId))
        {
            return WeaponTable[weaponId];
        }
        return null;
    }
}