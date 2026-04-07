using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
public class InventoryUI : MonoBehaviour
{
    [Header("Network")]
    [SerializeField] private GameServerClient gameServerClient;

    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel; // 인벤토리 전체 창
    [SerializeField] private Transform contentTransform;  // 프리팹들이 생성될 부모 (ScrollView의 Content)
    [SerializeField] private GameObject itemPrefab;       // 생성할 아이템 버튼 프리팹 (Button + TextMeshPro 포함)

    // 이전에 생성된 목록을 지우기 위한 리스트
    private List<GameObject> _spawnedItems = new List<GameObject>();
    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }
    private void Update()
    {
        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseInventory();
            }
        }
    }
    public void CloseInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            Debug.Log("인벤토리 창을 닫았습니다.");
        }
    }
    // '인벤토리 열기' 버튼 이벤트에 연결할 함수
    public async void OnClickOpenInventory()
    {
        inventoryPanel.SetActive(true);

        // 기존에 남아있던 목록 청소
        foreach (var item in _spawnedItems) Destroy(item);
        _spawnedItems.Clear();

        Debug.Log("인벤토리 목록을 서버에 요청합니다...");
        InventoryResponse response = await gameServerClient.RequestInventoryAsync();

        if (response.Success && response.Items != null)
        {
            foreach (var itemData in response.Items)
            {
                // 프리팹 생성
                GameObject newObj = Instantiate(itemPrefab, contentTransform);
                _spawnedItems.Add(newObj);

                // 프리팹 내부의 텍스트와 버튼 컴포넌트 찾기 (구조에 맞게 수정)
                TextMeshProUGUI nameText = newObj.GetComponentInChildren<TextMeshProUGUI>();
                Button equipButton = newObj.GetComponent<Button>();

                nameText.text = itemData.WeaponName;

                // 버튼을 클릭했을 때 이 무기를 장착하도록 이벤트 연결
                int currentWeaponId = itemData.WeaponId; // 람다식 클로저 변수 캡처 이슈 방지
                equipButton.onClick.AddListener(() => OnClickEquipItem(currentWeaponId, itemData.WeaponName));
            }
        }
        else
        {
            Debug.LogError($"[인벤토리 오류] {response.Message}");
        }
    }

    // 아이템 클릭(장착) 시 호출되는 함수
    private async void OnClickEquipItem(int weaponId, string weaponName)
    {
        Debug.Log($"서버에 '{weaponName}' 장착을 요청합니다... (ID: {weaponId})");

        EquipResponse response = await gameServerClient.RequestEquipAsync(weaponId);

        if (response.Success)
        {
            Debug.Log($"🎉 [장착 완료] {weaponName} 장착 성공!");

            // 1. DataManager에서 해당 무기 ID의 스탯 정보를 가져옵니다.
            WeaponStatData newWeaponStat = DataManager.Instance.GetWeaponStat(weaponId);

            if (newWeaponStat != null)
            {
                // 2. 씬에 있는 AutoShooter를 찾아서 스탯을 덮어씌우라고 명령합니다.
                AutoShooter playerShooter = FindAnyObjectByType<AutoShooter>();

                if (playerShooter != null)
                {
                    playerShooter.EquipWeapon(newWeaponStat);
                }
            }
            else
            {
                Debug.LogError("해당 무기의 데이터가 데이터 매니저에 없습니다!");
            }
        }
        else
        {
            Debug.LogError($"[장착 실패] {response.Message}");
        }
    }
}