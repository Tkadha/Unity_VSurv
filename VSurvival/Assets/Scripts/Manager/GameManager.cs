using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Lobby,
        Playing
    }

    public GameState State { get; private set; } = GameState.Lobby;

    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private ExperienceManager experienceManager;

    [Header("UI References")]
    [SerializeField] private LobbyUI lobbyUI;
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private LevelUpUI levelUpUI;
    [SerializeField] private UpgradeFeedbackUI upgradeFeedbackUI;

    [Header("Reset")]
    [SerializeField] private Vector2 playerResetPos = Vector2.zero;

    [Header("Network")]
    [SerializeField] private GameServerClient gameServerClient;
    [SerializeField] private string playerName = "Player";
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
        EnterLobbyState();
    }
    public async void RequestStartGame()
    {
        if (gameServerClient == null)
        {
            Debug.LogError("[GameManager] GameServerClient 참조가 없습니다.");
            return;
        }

        Debug.Log("[GameManager] 서버에 게임 시작 요청 전송");

        StartGameResponse response = await gameServerClient.RequestStartGameAsync(playerName);

        if (response.Success)
        {
            Debug.Log("[GameManager] 서버 승인 성공, 게임 시작");
            StartGame();
        }
        else
        {
            Debug.LogWarning($"[GameManager] 서버 승인 실패: {response.Message}");
        }
    }

    public void StartGame()
    {
        if (State == GameState.Playing)
            return;

        ResetRunState();
        ResetGameplayUI();

        if (playerTransform != null)
        {
            playerTransform.position = new Vector3(
                playerResetPos.x,
                playerResetPos.y,
                playerTransform.position.z
            );
        }

        if (enemySpawner != null)
        {
            enemySpawner.ClearAllEnemies();
            enemySpawner.SetSpawning(true);
        }

        State = GameState.Playing;
        ApplyUIByState();
    }

    public async void GameOver()
    {
        if (State != GameState.Playing) return;

        if (enemySpawner != null)
        {
            enemySpawner.SetSpawning(false);
            enemySpawner.ClearAllEnemies();
        }
        Debug.Log("[GameManager] 서버에 게임 종료 보고 및 초기화 대기 중...");
        var response = await gameServerClient.RequestEndGameAsync();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        if (response != null && response.Success)
        {
            Debug.Log("[GameManager] 서버 상태 초기화 완료. 로비로 이동합니다.");
            if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();
            ResetRunState();
            ResetGameplayUI();

            if (playerTransform != null) {
                playerTransform.position = new Vector3(
                playerResetPos.x,
                playerResetPos.y,
                playerTransform.position.z
            );
            }

            EnterLobbyState();
        }
        else
        {
            Debug.LogError("[GameManager] 서버 초기화 실패. 상태 불일치 발생 가능성 높음.");
        }      
    }

    private void ResetRunState()
    {
        if (playerStats != null)
            playerStats.ResetStats();

        if (experienceManager != null)
            experienceManager.ResetXp();

        if (playerHealth != null)
            playerHealth.ResetHealth();

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();
    }
    private void ResetGameplayUI()
    {
        if (levelUpUI != null)
            levelUpUI.Hide();

        if (upgradeFeedbackUI != null)
            upgradeFeedbackUI.HideImmediate();

        Time.timeScale = 1f;
    }

    private void EnterLobbyState()
    {
        State = GameState.Lobby;

        if (enemySpawner != null)
            enemySpawner.SetSpawning(false);

        ApplyUIByState();
    }

    private void ApplyUIByState()
    {
        bool isLobby = State == GameState.Lobby;
        bool isPlaying = State == GameState.Playing;

        if (lobbyUI != null)
        {
            if (isLobby) lobbyUI.Show();
            else lobbyUI.Hide();
        }

        if (hudRoot != null)
            hudRoot.SetActive(isPlaying);

        if (levelUpUI != null)
            levelUpUI.Hide();

        if (upgradeFeedbackUI != null)
            upgradeFeedbackUI.HideImmediate();
    }

    public bool IsLobbyState()
    {
        return State == GameState.Lobby;
    }

    public bool IsPlayingState()
    {
        return State == GameState.Playing;
    }
}