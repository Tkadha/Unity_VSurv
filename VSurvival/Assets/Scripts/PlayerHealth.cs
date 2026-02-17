using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHp = 5;

    public int CurrentHp { get; private set; }

    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        CurrentHp = maxHp;
    }

    public void ResetHealth()
    {
        CurrentHp = maxHp;
    }

    public void TakeDamage(int amount)
    {
        if (CurrentHp <= 0) return;

        CurrentHp -= amount;
        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            gameManager.GameOver();
        }
    }
}
