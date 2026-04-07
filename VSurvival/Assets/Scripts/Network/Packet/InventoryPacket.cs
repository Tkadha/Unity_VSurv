using System;
using System.Collections.Generic;

[Serializable]
public class InventoryRequest { }

[Serializable]
public class InventoryItem
{
    public int WeaponId;
    public string WeaponName;
}

[Serializable]
public class InventoryResponse
{
    public bool Success;
    public List<InventoryItem> Items;
    public string Message;
}

[Serializable]
public class EquipRequest
{
    public int WeaponId;
}

[Serializable]
public class EquipResponse
{
    public bool Success;
    public int EquippedWeaponId;
    public string Message;
}