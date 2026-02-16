using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 3f;

    [Header("Avoid Overlap")]
    [SerializeField] private float minDistanceFromPlayer = 2.0f;   // 플레이어 근처 스폰 금지
    [SerializeField] private float spawnCheckRadius = 0.35f;        // 스폰 지점 겹침 검사 반경
    [SerializeField] private LayerMask overlapMask;                // Enemy/Obstacle 레이어 등

    [Header("References")]
    [SerializeField] private BoxCollider2D spawnArea;              // IsTrigger 권장

    private Transform player;
    private float timer;

    private void Awake()
    {
        if (spawnArea == null)
            spawnArea = GetComponent<BoxCollider2D>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void Update()
    {
        if (enemyPrefab == null || spawnArea == null || player == null)
            return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnEnemy();
        }
    }

    private void TrySpawnEnemy()
    {
        // 여러 번 시도해서 “겹치지 않는” 지점을 찾기
        const int maxAttempts = 20;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 pos = GetRandomPointInBounds(spawnArea.bounds);

            // 1) 플레이어와 너무 가까우면 스킵
            if (Vector2.Distance(pos, player.position) < minDistanceFromPlayer)
                continue;

            // 2) 해당 지점에 이미 뭔가 있으면 스킵 (적/장애물 등)
            Collider2D hit = Physics2D.OverlapCircle(pos, spawnCheckRadius, overlapMask);
            if (hit != null)
                continue;

            Instantiate(enemyPrefab, pos, Quaternion.identity);
            return;
        }

        // 여기까지 왔으면 스폰 위치를 못 찾은 것 (맵이 너무 좁거나 overlapMask 설정 문제일 수 있음)
        // Debug.LogWarning("Failed to find valid spawn position.");
    }

    private Vector2 GetRandomPointInBounds(Bounds b)
    {
        return new Vector2(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y)
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnArea == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(spawnArea.bounds.center, spawnArea.bounds.size);
    }
}
