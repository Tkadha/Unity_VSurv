using System;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [Header("Experience")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private float currentXp = 0f;
    [SerializeField] private float requiredXp = 10f;

    [Header("Level Curve")]
    [SerializeField] private float baseRequiredXp = 5f;
    [SerializeField] private float requiredXpGrowth = 1.5f;

    [Header("Upgrade")]
    [SerializeField] private LevelUpUI levelUpUI;
    [SerializeField] private UpgradeFeedbackUI upgradeFeedbackUI;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerHealth playerHealth;
    public int CurrentLevel => currentLevel;
    public float CurrentXp => currentXp;
    public float RequiredXp => requiredXp;

    private void Awake()
    {
        requiredXp = CalculateRequiredXp(currentLevel);
    }

    private void OnEnable()
    {
        if (levelUpUI != null)
            levelUpUI.OnUpgradeSelected += HandleUpgradeSelected;
    }

    private void OnDisable()
    {
        if (levelUpUI != null)
            levelUpUI.OnUpgradeSelected -= HandleUpgradeSelected;

        Time.timeScale = 1f;
    }

    public void AddXp(float amount)
    {
        if (amount <= 0f)
            return;

        float finalAmount = amount;

        if (playerStats != null)
            finalAmount *= playerStats.XpGainMultiplier;

        currentXp += finalAmount;

        while (currentXp >= requiredXp)
        {
            currentXp -= requiredXp;
            LevelUp();
        }
    }

    public void ResetXp()
    {
        currentLevel = 1;
        currentXp = 0f;
        requiredXp = CalculateRequiredXp(currentLevel);

        Time.timeScale = 1f;

        if (levelUpUI != null && levelUpUI.IsShowing)
            levelUpUI.Hide();
    }

    private void LevelUp()
    {
        currentLevel++;
        requiredXp = CalculateRequiredXp(currentLevel);
        OnLevelUp();
    }

    private void OnLevelUp()
    {
        UpgradeType[] options = GetRandomUpgradeOptions(3);

        if (levelUpUI != null)
        {
            Time.timeScale = 0f;
            levelUpUI.Show(options);
        }
        else
        {
            Debug.LogWarning("ExperienceManager: LevelUpUI 참조가 없습니다.");
        }
    }

    private float CalculateRequiredXp(int level)
    {
        if (level <= 1)
            return baseRequiredXp;

        return Mathf.Round(baseRequiredXp * Mathf.Pow(requiredXpGrowth, level - 1));
    }

    private UpgradeType[] GetRandomUpgradeOptions(int count)
    {
        UpgradeType[] allTypes = (UpgradeType[])Enum.GetValues(typeof(UpgradeType));
        List<UpgradeType> pool = new List<UpgradeType>(allTypes);

        int resultCount = Mathf.Min(count, pool.Count);
        UpgradeType[] result = new UpgradeType[resultCount];

        for (int i = 0; i < resultCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, pool.Count);
            result[i] = pool[randomIndex];
            pool.RemoveAt(randomIndex);
        }

        return result;
    }

    private void HandleUpgradeSelected(UpgradeType upgradeType)
    {
        ApplyUpgrade(upgradeType);
        ShowUpgradeFeedback(upgradeType);
        Time.timeScale = 1f;
    }
    private void ShowUpgradeFeedback(UpgradeType upgradeType)
    {
        if (upgradeFeedbackUI == null)
            return;

        upgradeFeedbackUI.ShowMessage(GetUpgradeFeedbackMessage(upgradeType));
    }
    private string GetUpgradeFeedbackMessage(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.MoveSpeed:
                return "이동 속도 증가 (+20%) 적용";

            case UpgradeType.AttackDamage:
                return "공격력 증가 (+20%) 적용";

            case UpgradeType.AttackRate:
                return "공격 속도 증가 (+20%) 적용";

            case UpgradeType.MaxHealth:
                return "최대 체력 증가 (+20%) 적용";

            case UpgradeType.XpGain:
                return "경험치 획득량 증가 (+20%) 적용";

            default:
                return "업그레이드 적용";
        }
    }
    private void ApplyUpgrade(UpgradeType upgradeType)
    {
        if (playerStats == null)
        {
            Debug.LogWarning("ExperienceManager: PlayerStats 참조가 없습니다.");
            return;
        }

        switch (upgradeType)
        {
            case UpgradeType.MoveSpeed:
                playerStats.IncreaseMoveSpeed(0.10f);
                break;

            case UpgradeType.AttackDamage:
                playerStats.IncreaseAttackDamage(0.20f);
                break;

            case UpgradeType.AttackRate:
                playerStats.IncreaseAttackRate(0.15f);
                break;

            case UpgradeType.MaxHealth:
                playerStats.IncreaseMaxHealth(0.20f);

                if (playerHealth != null)
                    playerHealth.RefreshMaxHealth(false);
                break;

            case UpgradeType.XpGain:
                playerStats.IncreaseXpGain(0.15f);
                break;
        }
    }
}