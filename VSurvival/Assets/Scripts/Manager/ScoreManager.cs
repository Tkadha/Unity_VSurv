using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int CurrentScore { get; private set; }

    private void Awake()
    {
        ResetScore();
    }

    public void AddScore(int amount)
    {
        CurrentScore += amount;
    }

    public void ResetScore()
    {
        CurrentScore = 0;
    }
}