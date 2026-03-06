using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [Header("XP")]
    [SerializeField] private int currentXp = 0;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int requiredXp = 10;

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

    public void AddXp(int amount)
    {
        currentXp += amount;
    }

    public void ResetXp()
    {
        currentXp = 0;
        currentLevel = 1;
        requiredXp = 10;
    }
}