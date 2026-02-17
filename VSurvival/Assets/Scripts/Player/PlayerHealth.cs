using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHp = 5;
    public int CurrentHp { get; private set; }

    [Header("Invincibility")]
    [SerializeField] private float invincibleDuration = 1.0f; // 무적시간(초)
    [SerializeField] private float blinkPerSecond = 5.0f;     // 1초당 깜빡임(토글) 횟수
    [SerializeField] private bool enableBlink = true;

    private bool isInvincible;
    private Coroutine invincibleCo;
    private SpriteRenderer sr;
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        sr = GetComponentInChildren<SpriteRenderer>();
        CurrentHp = maxHp;
    }

    public bool IsInvincible => isInvincible;

    public void ResetHealth()
    {
        CurrentHp = maxHp;
        StopInvincibility();
    }

    public void TakeDamage(int amount)
    {
        if (CurrentHp <= 0) return;

        // 무적이면 데미지 무시
        if (isInvincible) return;

        CurrentHp -= amount;

        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            StopInvincibility();
            gameManager.GameOver();
            return;
        }

        // 데미지 받으면 무적 시작
        StartInvincibility();
    }

    private void StartInvincibility()
    {
        if (invincibleDuration <= 0f) return;

        if (invincibleCo != null)
            StopCoroutine(invincibleCo);

        invincibleCo = StartCoroutine(InvincibilityRoutine());
    }

    private void StopInvincibility()
    {
        isInvincible = false;

        if (invincibleCo != null)
        {
            StopCoroutine(invincibleCo);
            invincibleCo = null;
        }

        if (sr != null) sr.enabled = true; // 깜빡임 종료 시 항상 보이게
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        float interval = (blinkPerSecond > 0f) ? (1f / blinkPerSecond) : 0.2f;
        float endTime = Time.time + invincibleDuration;

        while (Time.time < endTime)
        {
            if (enableBlink && sr != null)
                sr.enabled = !sr.enabled;

            yield return new WaitForSeconds(interval);
        }

        if (sr != null) sr.enabled = true;
        isInvincible = false;
        invincibleCo = null;
    }
}
