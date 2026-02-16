using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChase2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.5f;

    private Rigidbody2D rb;
    private Transform target;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Player 태그로 찾기 (Player에 Tag=Player 설정 필요)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = ((Vector2)target.position - rb.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
    }
}
