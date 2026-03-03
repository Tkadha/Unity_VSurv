using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private int maxHp;
    private int currentHp;

    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;

    public void InitializeHealth(int hp)
    {
        maxHp = hp;
        currentHp = hp;
    }

    public void TakeDamage(int damage)
    {
        if (currentHp <= 0) return;

        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}