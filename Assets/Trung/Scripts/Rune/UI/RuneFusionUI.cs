using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; // Yêu cầu đã Import DoTween vào Project

public class RuneFusionUI : MonoBehaviour
{
    public static RuneFusionUI Instance;

    [Header("Panel Root")]
    [SerializeField] private GameObject fusionPanelRoot;

    [Header("Fusion Slots (3 Ô chứa GameObject Thẻ Ngọc)")]
    [SerializeField] private Transform[] ingredientSlots = new Transform[3];
    [SerializeField] private Image[] slotBgImages = new Image[3];

    [Header("Center Point (Tâm gộp hiệu ứng)")]
    [SerializeField] private Transform fusionCenterPoint;

    [Header("Action Buttons")]
    [SerializeField] private Button fuseButton;
    [SerializeField] private Button clearAllButton;

    [Header("Status Text")]
    [SerializeField] private TMP_Text chanceText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text costText;

    [Header("Prefab Sample Display")]
    [SerializeField] private RuneCardUI cardPrefabSample;

    // Danh sách lưu trữ nguyên liệu
    private List<RuneData> selectedRunes = new List<RuneData>();
    private List<RuneCardUI> spawnedVisualCards = new List<RuneCardUI>();
    private bool isAnimating = false;

    // Quản lý thực thể thẻ bài phần thưởng sau khi dung hợp thành công
    private RuneCardUI rewardCardInstance = null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (fuseButton != null) fuseButton.onClick.AddListener(OnFuseButtonClicked);
        if (clearAllButton != null) clearAllButton.onClick.AddListener(ClearFusionSlots);
        
        UpdateFusionPanelVisual();
    }

    /// <summary>
    /// Hàm được gọi khi người chơi nhấp chọn một viên ngọc từ hòm đồ lưới
    /// </summary>
    public void AddRuneToFusion(RuneData runeData)
    {
        if (isAnimating) return;

        // Nếu trên màn hình đang có viên ngọc phần thưởng của lượt trước, tự dọn đi để bắt đầu lượt mới
        ClearRewardCard();

        // 1. Kiểm tra nếu viên ngọc này đã được thêm vào trước đó rồi thì bỏ qua
        if (selectedRunes.Exists(r => r.runeID == runeData.runeID)) return;

        // 2. Không cho phép bỏ quá 3 viên
        if (selectedRunes.Count >= 3)
        {
            if (resultText != null) resultText.text = "<color=#FF4C4C>Ingredient slots are full!</color>";
            if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Material slots are full!", Color.red);
            return;
        }

        // 3. Kiểm tra tính đồng nhất về độ hiếm của nguyên liệu
        if (selectedRunes.Count > 0 && selectedRunes[0].runeRarity != runeData.runeRarity)
        {
            if (resultText != null) resultText.text = "<color=#FFFF66>Material rarity must be identical!</color>";
            if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Runes must be of the same rarity!", Color.yellow);
            return;
        }

        if (runeData.runeRarity == RuneRarity.Legendary)
        {
            if (resultText != null) resultText.text = "<color=#FFFF66>Legendary runes cannot be fused further!</color>";
            if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Legendary tier has reached maximum level!", Color.yellow);
            return;
        }

        // 4. Đưa vào danh sách và tạo Object hiển thị mô phỏng lên ô trống tương ứng
        selectedRunes.Add(runeData);
        int currentSlotIndex = selectedRunes.Count - 1;

        if (cardPrefabSample != null && ingredientSlots[currentSlotIndex] != null)
        {
            RuneCardUI visualCard = Instantiate(cardPrefabSample, ingredientSlots[currentSlotIndex]);
            visualCard.Setup(runeData, false);
            
            if (visualCard.GetComponent<Collider2D>() != null) visualCard.GetComponent<Collider2D>().enabled = false;

            // Ép lại trục RectTransform để thẻ bài nằm chính giữa tâm của ô Slot trống
            RectTransform rect = visualCard.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero; 
                rect.localPosition = Vector3.zero;
            }
            
            // Cho thẻ bài nhỏ lại 
            float targetScale = 0.5f; 
            visualCard.transform.localScale = Vector3.zero;
            visualCard.transform.DOScale(new Vector3(targetScale, targetScale, targetScale), 0.25f).SetEase(Ease.OutBack);

            spawnedVisualCards.Add(visualCard);
        }

        UpdateFusionPanelVisual();
    }

    public void ClearFusionSlots()
    {
        if (isAnimating) return;

        selectedRunes.Clear();
        for (int i = spawnedVisualCards.Count - 1; i >= 0; i--)
        {
            if (spawnedVisualCards[i] != null) Destroy(spawnedVisualCards[i].gameObject);
        }
        spawnedVisualCards.Clear();
        
        // Dọn sạch cả thẻ phần thưởng khi bấm nút Clear hòm đồ hoặc đổi Panel
        ClearRewardCard();
        
        if (resultText != null) resultText.text = "Select 3 runes of the same rarity to begin fusion...";
        UpdateFusionPanelVisual();
    }

    private void ClearRewardCard()
    {
        if (rewardCardInstance != null)
        {
            Destroy(rewardCardInstance.gameObject);
            rewardCardInstance = null;
        }
    }

private void UpdateFusionPanelVisual()
{
    if (selectedRunes.Count == 0)
    {
        if (chanceText != null) chanceText.text = "Success Rate: --%";
        if (costText != null) costText.text = "Cost: 0 Shards"; 
        if (fuseButton != null) fuseButton.interactable = false;
        return;
    }

    RuneRarity currentRarity = selectedRunes[0].runeRarity;
    
    if (chanceText != null)
    {
        float rate = currentRarity == RuneRarity.Common ? 85f : currentRarity == RuneRarity.Rare ? 60f : 35f;
        chanceText.text = $"Success Rate: <color=green>{rate}%</color>";
    }

    if (costText != null)
    {
        int cost = currentRarity == RuneRarity.Common ? 100 : currentRarity == RuneRarity.Rare ? 300 : 800;
        costText.text = $"Cost: <color=yellow>{cost} Shards</color>"; 
    }

    if (fuseButton != null)
    {
        fuseButton.interactable = (selectedRunes.Count == 3);
    }
}
    /// <summary>
    /// Hàm tự động điền nguyên liệu thông minh gán vào nút AutoFillButton ngoài giao diện
    /// </summary>
    public void AutoFillIngredients()
    {
        if (isAnimating || RuneInventoryManager.Instance == null) return;

        // 1. Dọn sạch các ô chứa hiện tại để tính toán bộ mồi mới
        ClearFusionSlots();

        // 2. Lấy toàn bộ danh sách ngọc thực tế trong túi đồ
        List<RuneData> allRunes = RuneInventoryManager.Instance.runes;

        // 3. Phân loại ngọc hợp lệ dựa trên cấu trúc bộ lọc RuneFilterPanel hiện tại
        List<RuneData> filteredRunes = new List<RuneData>();
        foreach (RuneData r in allRunes)
        {
            if (r == null || r.runeRarity == RuneRarity.Legendary) continue; // Ngọc Legendary không được đập tiếp

            // Nếu bảng bộ lọc đang mở, chỉ lấy ngọc vượt qua điều kiện lọc. Nếu bảng đóng, lấy tuốt.
            if (RuneFilterPanel.Instance != null)
            {
                if (RuneFilterPanel.Instance.EvaluateRuneFilter(r)) filteredRunes.Add(r);
            }
            else
            {
                filteredRunes.Add(r);
            }
        }

        // 4. Thuật toán tìm nhóm độ hiếm hợp lệ để tự điền (Ưu tiên Common -> Rare -> Epic)
        RuneRarity[] checkOrder = { RuneRarity.Common, RuneRarity.Rare, RuneRarity.Epic };
        RuneRarity selectedTargetRarity = RuneRarity.Common;
        bool foundValidGroup = false;

        foreach (RuneRarity targetRarity in checkOrder)
        {
            // Đếm xem trong danh sách ngọc đang hiển thị/lọc có đủ 3 viên thuộc độ hiếm này không
            int matchCount = 0;
            foreach (RuneData r in filteredRunes)
            {
                if (r.runeRarity == targetRarity) matchCount++;
            }

            if (matchCount >= 3)
            {
                selectedTargetRarity = targetRarity;
                foundValidGroup = true;
                break; // Tìm thấy cấp hiếm thấp nhất đủ điều kiện -> Thoát vòng lặp để gán đồ
            }
        }

        // 5. Nếu không cấp độ hiếm nào gom đủ 3 viên rảnh rỗi
        if (!foundValidGroup)
        {
            if (resultText != null) resultText.text = "<color=#FF4C4C>AutoFill failed: Need at least 3 matching runes!</color>";
            if (LobbyNotifyManager.Instance != null) 
                LobbyNotifyManager.Instance.ShowNotify("Not enough matching material runes (minimum 3)!", Color.red);
            return;
        }

        // 6. Đổ 3 viên tìm được vào ô dung hợp mô phỏng
        int addedCount = 0;
        foreach (RuneData r in filteredRunes)
        {
            if (r.runeRarity == selectedTargetRarity)
            {
                AddRuneToFusion(r); // Gọi lại hàm nạp visual card có sẵn của bạn
                addedCount++;
                if (addedCount >= 3) break; // Đủ 3 viên nguyên liệu hợp chuẩn -> Hoàn tất
            }
        }

        if (LobbyNotifyManager.Instance != null)
        {
            LobbyNotifyManager.Instance.ShowNotify($"Auto-filled 3 {selectedTargetRarity} material runes!", Color.green);
        }
    }

    private void OnFuseButtonClicked()
    {
        if (selectedRunes.Count != 3 || isAnimating) return;

        List<string> ids = new List<string>();
        foreach (RuneData r in selectedRunes) ids.Add(r.runeID);

        StartFusionAnimationSequence(ids);
    }

    private void StartFusionAnimationSequence(List<string> ingredientIDs)
    {
        isAnimating = true;
        ClearRewardCard(); 
        
        if (resultText != null) resultText.text = "<color=#33FFFF>Channelling elemental particles...</color>";

        Sequence fusionSequence = DOTween.Sequence();

        // 1. Nhấc cả 3 viên ngọc mồi co cụm bay thẳng vào vị trí Tâm
        for (int i = 0; i < spawnedVisualCards.Count; i++)
        {
            if (spawnedVisualCards[i] == null) continue;
            
            fusionSequence.Join(spawnedVisualCards[i].transform.DOMove(fusionCenterPoint.position, 0.6f).SetEase(Ease.InQuad));
            fusionSequence.Join(spawnedVisualCards[i].transform.DOScale(new Vector3(0.3f, 0.3f, 0.3f), 0.6f));
            fusionSequence.Join(spawnedVisualCards[i].transform.DORotate(new Vector3(0, 0, 360f), 0.6f, RotateMode.FastBeyond360));
        }

        // 2. Khi trúng tâm, phát nổ và hiển thị kết quả
        fusionSequence.OnComplete(() =>
        {
            bool isSuccess;
            RuneData resultRune;
            
            bool execute = RuneFusionManager.Instance.TryFuseRunes(ingredientIDs, out isSuccess, out resultRune);

            // Xóa sạch nguyên liệu cũ
            foreach (RuneCardUI visual in spawnedVisualCards) if (visual != null) Destroy(visual.gameObject);
            spawnedVisualCards.Clear();
            selectedRunes.Clear();

            if (execute)
            {
                if (isSuccess && resultRune != null)
                {
                    if (resultText != null) resultText.text = $"<color=#00FFCC>FUSION SUCCESSFUL!\nForged: {resultRune.runeName}</color>";
                    if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Fusion successful! Higher tier rune acquired.", Color.green);
                    Debug.Log($"<color=#00FFCC><b>[ÉP NGỌC]</b> Triệu hồi thành công viên ngọc cấp cao mới: {resultRune.runeName}</color>");

                    Camera.main.transform.DOShakePosition(0.3f, 15f, 20);

                    if (cardPrefabSample != null)
                    {
                        rewardCardInstance = Instantiate(cardPrefabSample, fusionCenterPoint);
                        rewardCardInstance.Setup(resultRune, false);

                        // Ép viên phần thưởng nằm khít ngay tại vị trí Tâm kết quả
                        RectTransform rewardRect = rewardCardInstance.GetComponent<RectTransform>();
                        if (rewardRect != null)
                        {
                            rewardRect.anchoredPosition = Vector2.zero;
                            rewardRect.localPosition = Vector3.zero;
                        }

                        float rewardScale = 0.7f; 
                        rewardCardInstance.transform.localScale = Vector3.zero;
                        
                        rewardCardInstance.transform.DOScale(new Vector3(rewardScale * 1.3f, rewardScale * 1.3f, rewardScale * 1.3f), 0.4f).SetEase(Ease.OutElastic).OnComplete(() =>
                        {
                            if (rewardCardInstance != null) 
                                rewardCardInstance.transform.DOScale(new Vector3(rewardScale, rewardScale, rewardScale), 0.15f);
                        });
                    }
                }
                else
                {
                    if (resultText != null) resultText.text = "<color=#FF4C4C>FUSION FAILED!\nMaterials broke into shards.</color>";
                    if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Fusion failed! Ingredients shattered.", Color.red);
                    Debug.Log("<color=#FF3333><b>[ÉP NGỌC]</b> Đập đồ thất bại, nguyên liệu thô đã bốc cháy thành tro bụi.</color>");

                    fusionCenterPoint.DOShakePosition(0.4f, 25f, 30);
                }
            }

            isAnimating = false;
            UpdateFusionPanelVisual();
        });
    }
}