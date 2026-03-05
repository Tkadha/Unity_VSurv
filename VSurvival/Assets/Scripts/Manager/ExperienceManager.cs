using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public int CurrentXp { get; private set; }

    private void Awake()
    {
        ResetXp();
    }

    public void AddXp(int amount)
    {
        if (amount <= 0) return;
        CurrentXp += amount;
    }

    public void ResetXp()
    {
        CurrentXp = 0;
    }
}