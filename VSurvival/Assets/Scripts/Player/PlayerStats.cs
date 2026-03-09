using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeedMultiplier = 1f;

    [Header("Combat")]
    [SerializeField] private float attackDamageMultiplier = 1f;
    [SerializeField] private float attackRateMultiplier = 1f;

    [Header("Health")]
    [SerializeField] private float maxHealthMultiplier = 1f;

    [Header("Experience")]
    [SerializeField] private float xpGainMultiplier = 1f;

    public float MoveSpeedMultiplier => moveSpeedMultiplier;
    public float AttackDamageMultiplier => attackDamageMultiplier;
    public float AttackRateMultiplier => attackRateMultiplier;
    public float MaxHealthMultiplier => maxHealthMultiplier;
    public float XpGainMultiplier => xpGainMultiplier;

    public void IncreaseMoveSpeed(float amount)
    {
        moveSpeedMultiplier += amount;
    }

    public void IncreaseAttackDamage(float amount)
    {
        attackDamageMultiplier += amount;
    }

    public void IncreaseAttackRate(float amount)
    {
        attackRateMultiplier += amount;
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealthMultiplier += amount;
    }

    public void IncreaseXpGain(float amount)
    {
        xpGainMultiplier += amount;
    }

    public void ResetStats()
    {
        moveSpeedMultiplier = 1f;
        attackDamageMultiplier = 1f;
        attackRateMultiplier = 1f;
        maxHealthMultiplier = 1f;
        xpGainMultiplier = 1f;
    }
}