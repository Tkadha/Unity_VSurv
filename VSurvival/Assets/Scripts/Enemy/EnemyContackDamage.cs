using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        var hp = collision.collider.GetComponent<PlayerHealth>();
        if (hp != null) hp.TakeDamage(damage);
    }

    // 만약 Trigger 콜라이더를 쓴다면 아래로 교체
    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (!other.CompareTag("Player")) return;
    //     var hp = other.GetComponent<PlayerHealth>();
    //     if (hp != null) hp.TakeDamage(damage);
    // }
}
