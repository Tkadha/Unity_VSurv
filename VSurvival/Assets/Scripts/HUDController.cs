using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Score UI")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Health UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthPercentText;

    private int score = 0;

    private void Start()
    {
        // 지금은 점수 0 고정 출력
        SetScore(0);
        UpdateHealthUI();
    }

    private void Update()
    {
        // 가장 단순/안전: 매 프레임 갱신 (나중에 이벤트 방식으로 바꿔도 됨)
        UpdateHealthUI();
    }

    public void SetScore(int value)
    {
        score = value;
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
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
