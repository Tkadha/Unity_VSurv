using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 3f;

    [Header("Avoid Overlap")]
    [SerializeField] private float minDistanceFromPlayer = 2.0f;
    [SerializeField] private float spawnCheckRadius = 0.35f;
    [SerializeField] private LayerMask overlapMask;

    [Header("References")]
    [SerializeField] private BoxCollider2D spawnArea;

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
        if (enemyPrefab == null || spawnArea == null || player == null) return;

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

            GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            spawnedEnemies.Add(enemy);
            return;
        }
    }

    private Vector2 GetRandomPointInBounds(Bounds b)
    {
        return new Vector2(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y)
        );
    }
}
