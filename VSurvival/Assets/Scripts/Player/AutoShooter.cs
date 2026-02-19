using UnityEngine;

public class AutoShooter : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private Transform muzzle;

    [Header("Shoot Settings")]
    [SerializeField] private float fireInterval = 0.5f;
    [SerializeField] private float range = 8f;

    private float timer;

    private void Update()
    {
        if (projectilePool == null) return;

        timer += Time.deltaTime;
        if (timer < fireInterval) return;
        timer = 0f;

        Transform target = FindNearestEnemy();
        if (target == null) return;

        Vector3 origin = (muzzle != null) ? muzzle.position : transform.position;
        Vector2 dir = (target.position - origin);
        if (dir.sqrMagnitude < 0.0001f) return;

        var p = projectilePool.Get(origin, Quaternion.identity);
        if (p == null) return; // 풀 최대치 도달 시 생략

        p.Fire(dir);
    }

    private Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies == null || enemies.Length == 0) return null;

        Transform best = null;
        float bestDistSq = range * range;
        Vector3 pos = transform.position;

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null) continue;

            float d = (enemies[i].transform.position - pos).sqrMagnitude;
            if (d <= bestDistSq)
            {
                bestDistSq = d;
                best = enemies[i].transform;
            }
        }

        return best;
    }
}
