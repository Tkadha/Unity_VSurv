using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private ScoreManager scoreManager;

    [Header("Score UI")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Health UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthPercentText;

    private void Start()
    {
        UpdateScoreUI();
        UpdateHealthUI();
    }

    private void Update()
    {
        // 가장 단순/안전: 매 프레임 갱신 (나중에 이벤트 방식으로 바꿔도 됨)
        UpdateScoreUI();
        UpdateHealthUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreManager == null || scoreText == null) return;

        scoreText.text = $"Score: {scoreManager.CurrentScore}";
    }

    private void UpdateHealthUI()
    {
        if (playerHealth == null) return;

        int cur = playerHealth.CurrentHp;
        int max = playerHealth.MaxHp;

        float ratio = (max > 0) ? (float)cur / max : 0f;

        if (healthBar != null)
            healthBar.value = ratio;

        if (healthPercentText != null)
            healthPercentText.text = $"HP : {cur}/{max}";
    }
}
