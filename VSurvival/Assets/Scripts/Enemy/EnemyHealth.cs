using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyHealth : MonoBehaviour
{
    private int maxHp;
    private int currentHp;

    private ScoreManager scoreManager;
    private EnemyStats enemyStats;
    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;

    private void Awake()
    {
        scoreManager = FindFirstObjectByType<ScoreManager>();
        enemyStats = GetComponent<EnemyStats>();
    }

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
        if (scoreManager != null)
        {
            scoreManager.AddScore(GetScoreByEnemyType());
        }

        Destroy(gameObject);
    }

    private int GetScoreByEnemyType()
    {
        if (enemyStats == null)
            return 1;

        switch (enemyStats.Type)
        {
            case EnemyStats.EnemyType.Weak:
                return 1;

            case EnemyStats.EnemyType.Normal:
                return 2;

            case EnemyStats.EnemyType.Strong:
                return 3;

            default:
                return 1;
        }
    }
}