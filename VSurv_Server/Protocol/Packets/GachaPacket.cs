namespace VSurvServer.Protocol.Packets;

public class GachaRequest
{
    // 가챠 횟수 등 추가 가능하지만 지금은 1회 뽑기로 단순화
}

public class GachaResponse
{
    public bool Success { get; set; }
    public int WeaponId { get; set; }  
    public string WeaponName { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}