using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 2.0f;

    private Rigidbody2D rb;
    private ProjectilePool pool;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // 풀에서 생성할 때 연결
    public void SetPool(ProjectilePool p)
    {
        pool = p;
    }

    // 발사 방향 설정
    public void Fire(Vector2 dir)
    {
        dir = dir.normalized;

        // 활성화 재사용 시 잔상 제거(속도/회전 초기화)
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.linearVelocity = dir * speed;

        CancelInvoke(nameof(ReturnToPool));
        Invoke(nameof(ReturnToPool), lifeTime);

        // 진행 방향 회전(선택)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnDisable()
    {
        // 풀로 돌아갈 때 예약 Invoke 정리(안전)
        CancelInvoke();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (pool != null)
            pool.Return(this);
        else
            gameObject.SetActive(false); // 혹시 풀 미연결 상태 대비
    }
}
