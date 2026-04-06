using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject rootPanel;

    [Header("Network")]
    [SerializeField] private GameServerClient gameServerClient;

    [Header("Ranking UI")]
    [SerializeField] private Button rankingButton;
    [SerializeField] private TextMeshProUGUI rankingText;

    [Header("Gacha")]
    [SerializeField] private Button gachaButton;
    [SerializeField] private TextMeshProUGUI gachaText;

    private void Start()
    {
        if (rankingButton != null)
        {
            rankingButton.onClick.AddListener(OnRankingButtonClicked);
        }
        if (gachaButton != null)
        {
            gachaButton.onClick.AddListener(OnClickGachaButton);

        }
    }
    private void OnEnable()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsLobbyState())
        {
            //OnRankingButtonClicked();
        }
    }

    private async void OnRankingButtonClicked()
    {
        if (rankingText != null) rankingText.text = "랭킹 불러오는 중...";

        if (rankingButton != null) rankingButton.interactable = false;

        RankingResponse response = await gameServerClient.RequestRankingAsync();

        if (response.Success && response.TopRanks != null)
        {
            string rankString = "--- TOP 10 랭킹 ---\n";

            foreach (var rank in response.TopRanks)
            {
                rankString += $"{rank.Rank}위: {rank.Username} - {rank.Score}점\n";
            }

            if (rankingText != null) rankingText.text = rankString;
            Debug.Log("[LobbyUI] 랭킹 조회 성공\n" + rankString);
        }
        else
        {
            if (rankingText != null) rankingText.text = "랭킹을 불러오지 못했습니다.";
            Debug.LogError("[LobbyUI] 랭킹 조회 실패");
        }

        if (rankingButton != null) rankingButton.interactable = true;
    }
    private async void OnClickGachaButton()
    {
        if (gachaText != null) gachaText.text = "뽑는 중...";
        if (gachaButton != null) gachaButton.interactable = false;

        Debug.Log("가챠를 요청합니다...");

        GachaResponse response = await gameServerClient.RequestGachaAsync();

        if (response.Success)
        {
            Debug.Log($"🎉 [가챠 성공] 획득 무기: {response.WeaponName} ({response.Rarity})");
            // TODO: 여기서 획득 연출(UI 애니메이션)이나 사운드를 재생하면 됩니다.
            if (gachaText != null) gachaText.text = response.WeaponName +"를 획득했습니다.";
        }
        else
        {
            Debug.LogError($"[가챠 실패] {response.Message}");
        }
        if (gachaButton != null) gachaButton.interactable = true;

    }
    public void Show()
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);
    }

    public void Hide()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    public void OnClickStartButton()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance가 없어 게임을 시작할 수 없습니다.");
            return;
        }

        GameManager.Instance.RequestStartGame();
    }
}