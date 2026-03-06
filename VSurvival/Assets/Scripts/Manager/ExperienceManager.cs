using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [Header("Level")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int maxLevel = 99;

    [Header("XP")]
    [SerializeField] private int currentXp = 0;
    [SerializeField] private int requiredXp = 10;

    [Header("XP Settings")]
    [SerializeField] private int baseRequiredXp = 10;
    [SerializeField] private int requiredXpIncreasePerLevel = 5;

    public int CurrentXp => currentXp;
    public int CurrentLevel => currentLevel;
    public int RequiredXp => requiredXp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        requiredXp = CalculateRequiredXp(currentLevel);
    }

    public void AddXp(int amount)
    {
        if (amount <= 0)
            return;

        if (currentLevel >= maxLevel)
            return;

        currentXp += amount;
        ProcessLevelUp();
    }

    private void ProcessLevelUp()
    {
        while (currentLevel < maxLevel && currentXp >= requiredXp)
        {
            currentXp -= requiredXp;
            currentLevel++;
            requiredXp = CalculateRequiredXp(currentLevel);

            OnLevelUp();
        }

        if (currentLevel >= maxLevel)
        {
            currentLevel = maxLevel;
            currentXp = 0;
        }
    }

    private int CalculateRequiredXp(int level)
    {
        return baseRequiredXp + (level - 1) * requiredXpIncreasePerLevel;
    }

    private void OnLevelUp()
    {
        Debug.Log($"Level Up! Current Level : {currentLevel}");
    }

    public void ResetXp()
    {
        currentLevel = 1;
        currentXp = 0;
        requiredXp = CalculateRequiredXp(currentLevel);
    }
}