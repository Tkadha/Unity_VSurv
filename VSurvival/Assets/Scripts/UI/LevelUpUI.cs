using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    [Serializable]
    private class OptionView
    {
        public Button button;
        public TMP_Text titleText;
        public TMP_Text descriptionText;

        [HideInInspector] public UpgradeType upgradeType;
    }

    [Header("Root")]
    [SerializeField] private GameObject rootPanel;

    [Header("Options")]
    [SerializeField] private OptionView option1;
    [SerializeField] private OptionView option2;
    [SerializeField] private OptionView option3;

    public bool IsShowing => rootPanel != null && rootPanel.activeSelf;

    public event Action<UpgradeType> OnUpgradeSelected;

    private void Awake()
    {
        HideImmediate();
        BindButtonEvents();
    }

    private void BindButtonEvents()
    {
        BindOptionButton(option1);
        BindOptionButton(option2);
        BindOptionButton(option3);
    }

    private void BindOptionButton(OptionView option)
    {
        if (option == null || option.button == null)
            return;

        option.button.onClick.RemoveAllListeners();
        option.button.onClick.AddListener(() => SelectOption(option.upgradeType));
    }

    public void Show(UpgradeType[] upgradeTypes)
    {
        if (upgradeTypes == null || upgradeTypes.Length < 3)
        {
            Debug.LogError("LevelUpUI.Show()에는 최소 3개의 UpgradeType이 필요합니다.");
            return;
        }

        ApplyOption(option1, upgradeTypes[0]);
        ApplyOption(option2, upgradeTypes[1]);
        ApplyOption(option3, upgradeTypes[2]);

        if (rootPanel != null)
            rootPanel.SetActive(true);
    }

    public void Hide()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    private void HideImmediate()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    private void SelectOption(UpgradeType upgradeType)
    {
        OnUpgradeSelected?.Invoke(upgradeType);
        Hide();
    }

    private void ApplyOption(OptionView option, UpgradeType upgradeType)
    {
        if (option == null)
            return;

        option.upgradeType = upgradeType;

        if (option.titleText != null)
            option.titleText.text = GetTitle(upgradeType);

        if (option.descriptionText != null)
            option.descriptionText.text = GetDescription(upgradeType);
    }

    private string GetTitle(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.MoveSpeed:
                return "이동 속도 증가";

            case UpgradeType.AttackDamage:
                return "공격력 증가";

            case UpgradeType.AttackRate:
                return "공격 속도 증가";

            case UpgradeType.MaxHealth:
                return "최대 체력 증가";

            case UpgradeType.XpGain:
                return "경험치 획득량 증가";

            default:
                return "알 수 없음";
        }
    }

    private string GetDescription(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.MoveSpeed:
                return "플레이어 이동 속도 배수를 증가시킵니다.";

            case UpgradeType.AttackDamage:
                return "투사체 최종 데미지 배수를 증가시킵니다.";

            case UpgradeType.AttackRate:
                return "자동 발사 주기를 더 빠르게 만듭니다.";

            case UpgradeType.MaxHealth:
                return "최대 체력 배수를 증가시킵니다.";

            case UpgradeType.XpGain:
                return "경험치 획득량 배수를 증가시킵니다.";

            default:
                return string.Empty;
        }
    }
}