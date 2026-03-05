using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Hit Cooldown")]
    [SerializeField] private float hitCooldown = 0.2f;

    [Header("XP Drop")]
    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private int xpDropCount = 1;

    private int maxHp;
    private int currentHp;

    private float lastHitTime = -999f;

    private ScoreManager scoreManager;
    private EnemyStats enemyStats;

    public int MaxHp => maxHp;
    public int CurrentHp => currentHp;

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

        DropXpOrbs();

        Destroy(gameObject);
    }

    private void DropXpOrbs()
    {
        if (xpOrbPrefab == null) return;

        for (int i = 0; i < xpDropCount; i++)
        {
            Vector3 spawnPos = transform.position;

            // 여러 개 드롭할 때 약간 퍼지게
            spawnPos.x += Random.Range(-0.2f, 0.2f);
            spawnPos.y += Random.Range(-0.2f, 0.2f);

            Instantiate(xpOrbPrefab, spawnPos, Quaternion.identity);
        }
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