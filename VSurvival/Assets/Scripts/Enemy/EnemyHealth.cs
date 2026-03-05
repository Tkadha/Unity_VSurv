using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyHealth : MonoBehaviour
{
    [Header("Hit Cooldown")]
    [SerializeField] private float hitCooldown = 0.2f;

    private int maxHp;
    private int currentHp;

    private float lastHitTime = -999f;

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
        lastHitTime = -999f;
    }

    public void TakeDamage(int damage)
    {
        if (currentHp <= 0) return;
        if (!CanTakeDamage()) return;

        currentHp -= damage;
        lastHitTime = Time.time;

        if (currentHp <= 0)
        {
            Die();
        }
    }
    private bool CanTakeDamage()
    {
        return Time.time - lastHitTime >= hitCooldown;
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