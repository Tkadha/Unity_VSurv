namespace VSurvServer.Protocol.Packets;

public class InventoryRequest
{
    // 조회 요청은 특별한 파라미터가 필요 없습니다 (세션의 UserId 사용)
}

public class InventoryItem
{
    public int WeaponId { get; set; }
    public string WeaponName { get; set; } = string.Empty;
}

public class InventoryResponse
{
    public bool Success { get; set; }
    public List<InventoryItem> Items { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class EquipRequest
{
    public int WeaponId { get; set; } // 장착하고 싶은 무기의 ID
}

public class EquipResponse
{
    public bool Success { get; set; }
    public int EquippedWeaponId { get; set; }
    public string Message { get; set; } = string.Empty;
}