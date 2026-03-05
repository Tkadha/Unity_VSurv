using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 2.0f;
    [SerializeField] private int damage = 1;

    private Rigidbody2D rb;
    private ProjectilePool pool;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetPool(ProjectilePool p)
    {
        pool = p;
    }

    public void Fire(Vector2 dir)
    {
        dir = dir.normalized;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.linearVelocity = dir * speed;

        CancelInvoke(nameof(ReturnToPool));
        Invoke(nameof(ReturnToPool), lifeTime);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnDisable()
    {
        CancelInvoke();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (pool != null)
            pool.Return(this);
        else
            gameObject.SetActive(false);
    }
}