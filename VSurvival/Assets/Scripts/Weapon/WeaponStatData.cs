using System;

[Serializable]
public class WeaponStatData
{
    public int WeaponId;         // 무기 고유 ID (서버 DB와 동일한 번호)
    public string WeaponName;    // 무기 이름
    public float AttackPower;    // ➡️ AutoShooter의 projectileDamage로 들어갈 값
    public float FireInterval;   // ➡️ AutoShooter의 fireInterval로 들어갈 값
}