using UnityEngine;

public class AutoShooter : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private Transform muzzle;

    [Header("Refs")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Shoot Settings")]
    [SerializeField] private float fireInterval = 0.5f;
    [SerializeField] private float range = 8f;
    [SerializeField] private float projectileDamage = 1f;

    private float timer;

    private void Update()
    {
        if (projectilePool == null) return;

        float finalFireInterval = fireInterval;

        if (playerStats != null && playerStats.AttackRateMultiplier > 0f)
        {
            finalFireInterval /= playerStats.AttackRateMultiplier;
        }

        timer += Time.deltaTime;
        if (timer < finalFireInterval) return;
        timer = 0f;

        Transform target = FindNearestEnemy();
        if (target == null) return;

        Vector3 origin = (muzzle != null) ? muzzle.position : transform.position;
        Vector2 dir = (target.position - origin);
        if (dir.sqrMagnitude < 0.0001f) return;

        float finalDamage = projectileDamage;

        if (playerStats != null)
        {
            finalDamage *= playerStats.AttackDamageMultiplier;
        }

        var p = projectilePool.Get(origin, Quaternion.identity);
        if (p == null) return;

        p.Fire(dir, finalDamage);
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