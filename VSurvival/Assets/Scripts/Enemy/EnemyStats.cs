using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public enum EnemyType
    {
        Weak,
        Normal,
        Strong
    }

    [Header("Type")]
    [SerializeField] private EnemyType enemyType = EnemyType.Normal;

    [Header("HP Multiplier")]
    [SerializeField] private float hpMultiplier = 1.0f;

    public EnemyType Type => enemyType;
    public float HpMultiplier => hpMultiplier;
}