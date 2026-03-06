using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Waiting, Running }
    public GameState State { get; private set; } = GameState.Waiting;

    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Transform playerTransform;

    [Header("Reset")]
    [SerializeField] private Vector2 playerResetPos = Vector2.zero;

    private void Start()
    {
        // 시작 시 대기 상태 세팅
        EnterWaitingState();
    }

    private void Update()
    {
        // "어떤 키든 입력" 감지 (New Input System)
        bool anyKeyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;

        if (State == GameState.Waiting && anyKeyPressed)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        State = GameState.Running;

        // 플레이어 상태 초기화(체력, 무적 등)
        playerHealth.ResetHealth();

        // 스포너 시작
        enemySpawner.SetSpawning(true);
    }

    public void GameOver()
    {
        // Running 중에만 처리
        if (State != GameState.Running) return;

        // 스폰 중지
        enemySpawner.SetSpawning(false);

        // 적 전부 제거
        enemySpawner.ClearAllEnemies();

        // 플레이어 원위치
        playerTransform.position = new Vector3(playerResetPos.x, playerResetPos.y, playerTransform.position.z);

        // 경험치/레벨 초기화
        ExperienceManager.Instance.ResetXp();

        // 대기 상태로 복귀
        EnterWaitingState();
    }

    private void EnterWaitingState()
    {
        State = GameState.Waiting;

        // 대기 중에는 스폰 꺼두기
        enemySpawner.SetSpawning(false);
    }
}
