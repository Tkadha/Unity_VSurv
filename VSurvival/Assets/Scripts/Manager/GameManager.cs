using UnityEngine;
using System.Threading.Tasks;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Login,
        Lobby,
        Playing
    }

    public GameState State { get; private set; } = GameState.Login;

    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private ExperienceManager experienceManager;
    [SerializeField] private ScoreManager scoreManager;

    [Header("UI References")]
    [SerializeField] private GameObject authPanel;
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

    private async void Start()
    {
        EnterLoginState();
        await ConnectToServerAsync();
    }
    private async Task ConnectToServerAsync()
    {
        if (gameServerClient == null) return;

        Debug.Log("[GameManager] 서버 상시 연결 시도 중...");
        bool isConnected = await gameServerClient.ConnectAsync();

        if (isConnected)
            Debug.Log("[GameManager] 서버 자동 연결 성공! (게임 종료 시까지 유지)");
        else
            Debug.LogError("[GameManager] 서버 연결 실패. 서버가 켜져 있는지 확인하세요.");
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
        int finalScore = 0;
        if (scoreManager != null)
        {
            finalScore = scoreManager.CurrentScore;
        }

        Debug.Log($"[GameManager] 서버에 게임 종료 보고 (점수: {finalScore})...");

        var response = await gameServerClient.RequestEndGameAsync(finalScore);

        if (response != null && response.Success)
        {
            Debug.Log("[GameManager] 서버 상태 초기화 완료. 로비로 이동합니다.");
            if (scoreManager != null) scoreManager.ResetScore();
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
        if (playerStats != null) playerStats.ResetStats();
        if (experienceManager != null) experienceManager.ResetXp();
        if (playerHealth != null) playerHealth.ResetHealth();
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();
    }
    private void ResetGameplayUI()
    {
        if (levelUpUI != null) levelUpUI.Hide();
        if (upgradeFeedbackUI != null) upgradeFeedbackUI.HideImmediate();
        Time.timeScale = 1f;
    }

    public void OnLoginSuccess(string username)
    {
        playerName = username; // 접속한 유저 이름 저장
        Debug.Log($"[GameManager] 인증 성공. 유저명: {playerName}. 로비로 이동합니다.");
        EnterLobbyState();
    }

    public void EnterLoginState()
    {
        State = GameState.Login;
        ApplyUIByState();
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
        bool isLogin = State == GameState.Login;
        bool isLobby = State == GameState.Lobby;
        bool isPlaying = State == GameState.Playing;

        // 상태에 따라 패널 On/Off 스위칭
        if (authPanel != null) authPanel.SetActive(isLogin);

        if (lobbyUI != null)
        {
            if (isLobby) lobbyUI.Show();
            else lobbyUI.Hide();
        }

        if (hudRoot != null) hudRoot.SetActive(isPlaying);
        if (levelUpUI != null) levelUpUI.Hide();
        if (upgradeFeedbackUI != null) upgradeFeedbackUI.HideImmediate();
    }

    public bool IsLobbyState() => State == GameState.Lobby;
    public bool IsPlayingState() => State == GameState.Playing;

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
}