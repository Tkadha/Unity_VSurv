using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class XPOrb : MonoBehaviour
{
    [Header("XP")]
    [SerializeField] private int xpValue = 1;

    [Header("Magnet")]
    [SerializeField] private float detectRange = 2.5f;
    [SerializeField] private float moveSpeed = 6f;

    private Transform player;
    private ExperienceManager experienceManager;
    private bool isTrackingPlayer = false;

    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        experienceManager = FindFirstObjectByType<ExperienceManager>();
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // 일정 거리 안으로 들어오면 플레이어를 추적 시작
        if (!isTrackingPlayer && distance <= detectRange)
        {
            isTrackingPlayer = true;
        }

        // 추적 상태면 플레이어 방향으로 이동
        if (isTrackingPlayer)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (experienceManager != null)
        {
            experienceManager.AddXp(xpValue);
        }

        Destroy(gameObject);
    }
}