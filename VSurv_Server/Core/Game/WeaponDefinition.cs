namespace VSurvServer.Core.Game;

public class WeaponDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Power { get; set; }
    public float FireRate { get; set; }
    public string Rarity { get; set; } = string.Empty;
    public string PrefabName { get; set; } = string.Empty;
    public string AmmoType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}