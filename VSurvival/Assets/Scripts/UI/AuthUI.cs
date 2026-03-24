using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AuthUI : MonoBehaviour
{
    [Header("Network")]
    [SerializeField] private GameServerClient gameServerClient;

    [Header("UI References")]
    [SerializeField] private TMP_InputField idInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private TextMeshProUGUI statusText;

    private void Start()
    {
        registerButton.onClick.AddListener(OnRegisterButtonClicked);
        loginButton.onClick.AddListener(OnLoginButtonClicked);
    }

    private async void OnRegisterButtonClicked()
    {
        string id = idInputField.text;
        string pw = passwordInputField.text;

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(pw))
        {
            statusText.text = "아이디와 비밀번호를 입력하세요.";
            return;
        }

        registerButton.interactable = false;
        statusText.text = "서버와 통신 중...";

        RegisterResponse response = await gameServerClient.RequestRegisterAsync(id, pw);

        statusText.text = response.Message;

        if (response.Success)
        {
            Debug.Log("[AuthUI] 회원가입 성공!");

            GameManager.Instance.OnLoginSuccess(id);

            idInputField.text = "";
            passwordInputField.text = "";
        }
        else
        {
            Debug.LogWarning($"[AuthUI] 회원가입 실패: {response.Message}");
        }

        registerButton.interactable = true;
    }
    private async void OnLoginButtonClicked()
    {
        string id = idInputField.text;
        string pw = passwordInputField.text;

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(pw))
        {
            statusText.text = "아이디와 비밀번호를 입력하세요.";
            return;
        }

        SetButtonsInteractable(false);
        statusText.text = "로그인 중...";

        // 서버로 로그인 요청 전송
        LoginResponse response = await gameServerClient.RequestLoginAsync(id, pw);

        statusText.text = response.Message;

        if (response.Success)
        {
            Debug.Log($"[AuthUI] 로그인 성공! UserId: {response.UserId}");

            // GameManager에 유저 이름을 넘기고 로비로 이동시킴
            GameManager.Instance.OnLoginSuccess(id);

            idInputField.text = "";
            passwordInputField.text = "";
        }

        SetButtonsInteractable(true);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        registerButton.interactable = interactable;
        loginButton.interactable = interactable;
    }
}