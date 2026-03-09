using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int baseMaxHp = 5;
    public int CurrentHp { get; private set; }

    [Header("Refs")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Invincibility")]
    [SerializeField] private float invincibleDuration = 1.0f;
    [SerializeField] private float blinkPerSecond = 5.0f;
    [SerializeField] private bool enableBlink = true;

    private int maxHp;
    private bool isInvincible;
    private Coroutine invincibleCo;
    private SpriteRenderer sr;
    private GameManager gameManager;

    public int MaxHp => maxHp;
    public int Current => CurrentHp;
    public bool IsInvincible => isInvincible;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        sr = GetComponentInChildren<SpriteRenderer>();

        maxHp = CalculateMaxHp();
        CurrentHp = maxHp;
    }

    private int CalculateMaxHp()
    {
        float multiplier = 1f;

        if (playerStats != null)
        {
            multiplier = playerStats.MaxHealthMultiplier;
        }

        return Mathf.RoundToInt(baseMaxHp * multiplier);
    }

    public void ResetHealth()
    {
        maxHp = CalculateMaxHp();
        CurrentHp = maxHp;
        StopInvincibility();
    }
    public void RefreshMaxHealth(bool healToFull = false)
    {
        int oldMaxHp = maxHp;
        maxHp = CalculateMaxHp();

        if (healToFull)
        {
            CurrentHp = maxHp;
        }
        else
        {
            int hpDiff = maxHp - oldMaxHp;
            CurrentHp = Mathf.Clamp(CurrentHp + hpDiff, 0, maxHp);
        }
    }
    public void TakeDamage(int amount)
    {
        if (CurrentHp <= 0) return;
        if (isInvincible) return;

        CurrentHp -= amount;

        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            StopInvincibility();
            gameManager.GameOver();
            return;
        }

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

        if (sr != null) sr.enabled = true;
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