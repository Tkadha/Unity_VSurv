using System.Buffers.Text;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnEntry
    {
        public GameObject prefab;
        [Range(0f, 1f)] public float spawnWeight = 0.33f;
    }

    [Header("Spawn")]
    [SerializeField] private List<EnemySpawnEntry> enemyEntries = new List<EnemySpawnEntry>();
    [SerializeField] private float spawnInterval = 3f;

    [Header("Avoid Overlap")]
    [SerializeField] private float minDistanceFromPlayer = 2.0f;
    [SerializeField] private float spawnCheckRadius = 0.35f;
    [SerializeField] private LayerMask overlapMask;

    [Header("References")]
    [SerializeField] private BoxCollider2D spawnArea;

    [Header("HP Scaling")]
    [SerializeField] private int baseHp = 3;
    [SerializeField] private float hpGrowthPerSecond = 0.05f;

    private Transform player;
    private float timer;
    private bool spawningEnabled = false;

    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Awake()
    {
        if (spawnArea == null)
            spawnArea = GetComponent<BoxCollider2D>();

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    public void SetSpawning(bool enabled)
    {
        spawningEnabled = enabled;
        timer = 0f;
    }

    public void ClearAllEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] != null)
                Destroy(spawnedEnemies[i]);
        }
        spawnedEnemies.Clear();
    }

    private void Update()
    {
        if (!spawningEnabled) return;
        if (spawnArea == null || player == null) return;
        if (enemyEntries == null || enemyEntries.Count == 0) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnEnemy();
        }
    }

    private void TrySpawnEnemy()
    {
        const int maxAttempts = 20;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 pos = GetRandomPointInBounds(spawnArea.bounds);

            if (Vector2.Distance(pos, player.position) < minDistanceFromPlayer)
                continue;

            Collider2D hit = Physics2D.OverlapCircle(pos, spawnCheckRadius, overlapMask);
            if (hit != null)
                continue;

            GameObject selectedPrefab = GetRandomEnemyPrefab();
            if (selectedPrefab == null) return;

            GameObject enemy = Instantiate(selectedPrefab, pos, Quaternion.identity);
            spawnedEnemies.Add(enemy);

            InitializeEnemyHp(enemy);
            return;
        }
    }
    private GameObject GetRandomEnemyPrefab()
    {
        float totalWeight = 0f;

        for (int i = 0; i < enemyEntries.Count; i++)
        {
            if (enemyEntries[i].prefab != null)
                totalWeight += enemyEntries[i].spawnWeight;
        }

        if (totalWeight <= 0f) return null;

        float randomValue = Random.Range(0f, totalWeight);
        float current = 0f;

        for (int i = 0; i < enemyEntries.Count; i++)
        {
            if (enemyEntries[i].prefab == null) continue;

            current += enemyEntries[i].spawnWeight;
            if (randomValue <= current)
            {
                return enemyEntries[i].prefab;
            }
        }

        return enemyEntries[enemyEntries.Count - 1].prefab;
    }

    private void InitializeEnemyHp(GameObject enemy)
    {
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        EnemyStats stats = enemy.GetComponent<EnemyStats>();

        if (health == null || stats == null) return;

        float elapsedTime = Time.timeSinceLevelLoad;

        float timeScaledHp = baseHp + (elapsedTime * hpGrowthPerSecond);
        float finalHp = timeScaledHp * stats.HpMultiplier;

        int hp = Mathf.Max(1, Mathf.RoundToInt(finalHp));
        health.InitializeHealth(hp);
    }

    private Vector2 GetRandomPointInBounds(Bounds b)
    {
        return new Vector2(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y)
        );
    }
}
