using System;

[Serializable]
public class GachaRequest
{
    // 현재는 파라미터가 없으므로 비워둡니다.
}

[Serializable]
public class GachaResponse
{
    public bool Success;
    public int WeaponId;
    public string WeaponName;
    public string Rarity;
    public string Message;
}