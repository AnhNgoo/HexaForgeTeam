using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class LobbyBossSelectMenu : MenuBase
{
    public override MenuType menuType => MenuType.LobbyBossSelectMenu;

    [System.Serializable]
    public class BossSelectOption
    {
        public string bossName;
        public PoolType bossPoolType;
        public Button selectButton;
        public GameObject highlightObject;

        [Header("Video Preview Settings")]
        public VideoPlayer videoPlayer;
        public RawImage videoRawImage;
        public RenderTexture videoRenderTexture;

        [Header("Lock State")]
        public GameObject lockOverlay;
        public TMP_Text lockMessageText;
    }

    [Header("Boss Config List")]
    [SerializeField] private List<BossSelectOption> bossOptions = new List<BossSelectOption>();

    [Header("Wager Slider & Risk Tiers UI")]
    [SerializeField] private Slider wagerSlider;
    [SerializeField] private TMP_Text txtSelectedWagerInfo;

    [Header("Wager Costs & Multipliers Config")]
    [SerializeField] private int costTier1 = 50;
    [SerializeField] private int costTier2 = 150;
    [SerializeField] private int costTier3 = 400;

    [Header("Run Buff Toggles & Item Count Displays")]
    [SerializeField] private GameObject buffGroupRoot;
    [SerializeField] private Toggle toggleGoldBuff;
    [SerializeField] private TMP_Text txtGoldCount;
    [SerializeField] private Toggle toggleReviveBuff;
    [SerializeField] private TMP_Text txtReviveCount;
    [SerializeField] private Toggle toggleAtkBuff;
    [SerializeField] private TMP_Text txtAtkCount;

    [Header("UI Action Buttons")]
    [SerializeField] private Button btnConfirmStartRun;

    [Header("Map Debug Preview (Optional Text in Scene)")]
    [SerializeField] private TMP_Text txtSelectedMapDebug;

    private const string ItemGoldID = "ITEM_BUFF_GOLD";
    private const string ItemReviveID = "ITEM_BUFF_REVIVE";
    private const string ItemAtkID = "ITEM_BUFF_ATK";

    private const string ItemGoldName = "Lucky Cat";
    private const string ItemReviveName = "Medical Kit";
    private const string ItemAtkName = "Brawn Elixir";

    private int selectedBossIndex = 0;
    private string previewedRunMapName = "";
    private int selectedWagerAmount = 50;
    private float selectedMultiplier = 1.0f;
    private int currentTierIndex = 0;
    private Coroutine playVideoCoroutine;

    protected override void LoadComponent()
    {
        if (txtSelectedMapDebug == null)
        {
            txtSelectedMapDebug = transform.Find("TxtSelectedMapDebug")?.GetComponent<TMP_Text>();
        }
    }

    protected override void LoadComponentRuntime() { }

    private void Start()
    {
        if (btnConfirmStartRun != null)
        {
            btnConfirmStartRun.onClick.RemoveAllListeners();
            btnConfirmStartRun.onClick.AddListener(OnConfirmStartRun);
        }

        SetupWagerSlider();
        SetupBossButtons();
        SetupBuffToggles();
        UpdateMapDebugUI();
    }

    private void OnEnable()
    {
        if (wagerSlider != null)
        {
            EventTrigger trigger = wagerSlider.gameObject.GetComponent<EventTrigger>()
                ?? wagerSlider.gameObject.AddComponent<EventTrigger>();

            trigger.triggers.Clear();
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            entry.callback.AddListener((data) => { SnapToClosestTier(); });
            trigger.triggers.Add(entry);
        }
    }

    private void SetupBuffToggles()
    {
        if (toggleGoldBuff != null)
        {
            toggleGoldBuff.onValueChanged.RemoveAllListeners();
            toggleGoldBuff.onValueChanged.AddListener((isOn) => OnBuffToggleClicked(ItemGoldID, toggleGoldBuff, isOn));
        }

        if (toggleReviveBuff != null)
        {
            toggleReviveBuff.onValueChanged.RemoveAllListeners();
            toggleReviveBuff.onValueChanged.AddListener((isOn) => OnBuffToggleClicked(ItemReviveID, toggleReviveBuff, isOn));
        }

        if (toggleAtkBuff != null)
        {
            toggleAtkBuff.onValueChanged.RemoveAllListeners();
            toggleAtkBuff.onValueChanged.AddListener((isOn) => OnBuffToggleClicked(ItemAtkID, toggleAtkBuff, isOn));
        }
    }

    private void OnBuffToggleClicked(string itemID, Toggle toggle, bool isOn)
    {
        if (!isOn) return;

        int owned = InventoryItemManager.Instance != null ? InventoryItemManager.Instance.GetItemQuantity(itemID) : 0;
        if (owned <= 0)
        {
            toggle.SetIsOnWithoutNotify(false);
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify("Vật phẩm đã hết! Hãy mua thêm tại Cửa Hàng.", Color.yellow);
            }
        }
    }

    private void RefreshBuffTogglesState()
    {
        int goldQty = InventoryItemManager.Instance != null ? InventoryItemManager.Instance.GetItemQuantity(ItemGoldID) : 0;
        int reviveQty = InventoryItemManager.Instance != null ? InventoryItemManager.Instance.GetItemQuantity(ItemReviveID) : 0;
        int atkQty = InventoryItemManager.Instance != null ? InventoryItemManager.Instance.GetItemQuantity(ItemAtkID) : 0;

        if (txtGoldCount != null) txtGoldCount.SetTextSafe($"{ItemGoldName} x{goldQty}");
        if (txtReviveCount != null) txtReviveCount.SetTextSafe($"{ItemReviveName} x{reviveQty}");
        if (txtAtkCount != null) txtAtkCount.SetTextSafe($"{ItemAtkName} x{atkQty}");

        if (currentTierIndex == 0)
        {
            SetToggleActive(toggleGoldBuff, false);
            SetToggleActive(toggleReviveBuff, false);
            SetToggleActive(toggleAtkBuff, false);
        }
        else if (currentTierIndex == 1)
        {
            SetToggleActive(toggleGoldBuff, true);
            SetToggleActive(toggleReviveBuff, true);
            SetToggleActive(toggleAtkBuff, false);
        }
        else
        {
            SetToggleActive(toggleGoldBuff, true);
            SetToggleActive(toggleReviveBuff, true);
            SetToggleActive(toggleAtkBuff, true);
        }
    }

    private void SetToggleActive(Toggle toggle, bool isInteractable)
    {
        if (toggle == null) return;
        toggle.interactable = isInteractable;
        if (!isInteractable)
        {
            toggle.SetIsOnWithoutNotify(false);
        }
    }

    private void SetupWagerSlider()
    {
        if (wagerSlider != null)
        {
            wagerSlider.wholeNumbers = true;
            wagerSlider.minValue = 0;
            wagerSlider.maxValue = 2;
            wagerSlider.value = 0;
            wagerSlider.transition = Selectable.Transition.ColorTint;

            wagerSlider.onValueChanged.RemoveAllListeners();
            wagerSlider.onValueChanged.AddListener(OnWagerSliderChanged);

            OnWagerSliderChanged(0);
        }
    }

    private void SnapToClosestTier()
    {
        if (wagerSlider == null) return;

        int nearestTier = Mathf.RoundToInt(wagerSlider.value);
        DOTween.To(() => wagerSlider.value, x => wagerSlider.value = x, nearestTier, 0.15f)
              .SetEase(Ease.OutQuad)
              .SetUpdate(true);
    }

    private void OnWagerSliderChanged(float value)
    {
        currentTierIndex = Mathf.RoundToInt(value);

        switch (currentTierIndex)
        {
            case 0:
                selectedWagerAmount = costTier1;
                selectedMultiplier = 1.0f;
                if (txtSelectedWagerInfo != null)
                {
                    txtSelectedWagerInfo.text =
                        "<b>Tier: <color=#00FF00>Standard (Safe)</color></b> | Bet: <color=#00FFFF>" + costTier1 + " Gems</color> (x1.0)\n" +
                        "<color=#00FF00>• Benefits:</color> Standard combat, resources kept intact.\n" +
                        "<color=#FF5555>• Note:</color> No secondary buffs allowed.";
                }
                break;

            case 1:
                selectedWagerAmount = costTier2;
                selectedMultiplier = 1.5f;
                if (txtSelectedWagerInfo != null)
                {
                    txtSelectedWagerInfo.text =
                        "<b>Tier: <color=#FFFF00>Risky (Challenge)</color></b> | Bet: <color=#00FFFF>" + costTier2 + " Gems</color> (x1.5)\n" +
                        "<color=#00FF00>• Benefits:</color> EXP & Resource gains +50%.\n" +
                        "<color=#00FFFF>• Buffs:</color> Gold Boost & Phoenix Charm available.";
                }
                break;

            case 2:
                selectedWagerAmount = costTier3;
                selectedMultiplier = 2.5f;
                if (txtSelectedWagerInfo != null)
                {
                    txtSelectedWagerInfo.text =
                        "<b>Tier: <color=#FF3333>Nightmare (Hardcore)</color></b> | Bet: <color=#00FFFF>" + costTier3 + " Gems</color> (x2.5)\n" +
                        "<color=#00FF00>• Benefits:</color> Massive rewards (x2.5) & Rune Shards.\n" +
                        "<color=#00FFFF>• Buffs:</color> All 3 Combat Elixirs unlocked!";
                }
                break;
        }

        RefreshBuffTogglesState();
    }

    public override void Open(object data = null)
    {
        base.Open(data);

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowCurrencyOnly();
        }

        SetupBossButtons();
        RefreshBossLockStates();
        RerollPreviewMap();
        RefreshBuffTogglesState();

        HideAndStopAllVideos();

        SelectBoss(selectedBossIndex, isInitialOpen: true);
    }

    public override void Close()
    {
        if (playVideoCoroutine != null)
        {
            StopCoroutine(playVideoCoroutine);
            playVideoCoroutine = null;
        }

        HideAndStopAllVideos();
        base.Close();

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowFullHUD();
        }

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = false;
        }
    }

    public void RefreshBossLockStates()
    {
        bool isBoss2Unlocked = PlayerPrefs.GetInt("UNLOCKED_BOSS_DARKMAGE", 0) == 1;

        for (int i = 0; i < bossOptions.Count; i++)
        {
            var option = bossOptions[i];
            if (option == null) continue;

            bool isLocked = (option.bossPoolType == PoolType.EnemyDarkMageBoss) && !isBoss2Unlocked;

            if (option.lockOverlay != null) option.lockOverlay.SetActive(isLocked);
            if (option.selectButton != null) option.selectButton.interactable = !isLocked;
            if (option.lockMessageText != null) option.lockMessageText.gameObject.SetActive(isLocked);
        }
    }

    private void SetupBossButtons()
    {
        for (int i = 0; i < bossOptions.Count; i++)
        {
            int index = i;
            var option = bossOptions[index];
            if (option == null || option.selectButton == null) continue;

            if (option.videoPlayer != null)
            {
                option.videoPlayer.playOnAwake = false;
            }

            option.selectButton.onClick.RemoveAllListeners();
            option.selectButton.onClick.AddListener(() =>
            {
                SelectBoss(index, isInitialOpen: false);
            });
        }
    }

    private void PlaySelectedBossVideoOnly(int targetIndex)
    {
        if (playVideoCoroutine != null)
        {
            StopCoroutine(playVideoCoroutine);
        }

        playVideoCoroutine = StartCoroutine(PlayVideoRoutine(targetIndex));
    }

    private IEnumerator PlayVideoRoutine(int targetIndex)
    {
        for (int i = 0; i < bossOptions.Count; i++)
        {
            var opt = bossOptions[i];
            if (opt == null) continue;

            if (i != targetIndex)
            {
                if (opt.videoPlayer != null && opt.videoPlayer.isPlaying) opt.videoPlayer.Stop();
                if (opt.videoRawImage != null) opt.videoRawImage.gameObject.SetActive(false);
            }
        }

        if (targetIndex >= 0 && targetIndex < bossOptions.Count)
        {
            var currentOpt = bossOptions[targetIndex];
            if (currentOpt != null && currentOpt.videoPlayer != null)
            {
                if (currentOpt.videoRawImage != null) currentOpt.videoRawImage.gameObject.SetActive(true);
                if (!currentOpt.videoPlayer.gameObject.activeSelf) currentOpt.videoPlayer.gameObject.SetActive(true);

                if (currentOpt.videoRenderTexture != null) currentOpt.videoRenderTexture.Release();

                currentOpt.videoPlayer.Stop();
                currentOpt.videoPlayer.Prepare();

                while (!currentOpt.videoPlayer.isPrepared)
                {
                    yield return null;
                }

                currentOpt.videoPlayer.Play();
            }
        }
    }

    private void HideAndStopAllVideos()
    {
        for (int i = 0; i < bossOptions.Count; i++)
        {
            var opt = bossOptions[i];
            if (opt == null) continue;

            if (opt.videoPlayer != null) opt.videoPlayer.Stop();
            if (opt.videoRawImage != null) opt.videoRawImage.gameObject.SetActive(false);
            if (opt.videoRenderTexture != null) opt.videoRenderTexture.Release();
        }
    }

    public void RerollPreviewMap()
    {
        previewedRunMapName = GameSceneData.Instance != null
            ? GameSceneData.Instance.GetRandomRunSceneName()
            : "Run Scene";

        UpdateMapDebugUI();
    }

    private void ToggleForceMap()
    {
        if (GameSceneData.Instance == null) return;

        string map1 = GameSceneData.Instance.GetSceneName(SceneType.RunGameplay);
        string map2 = GameSceneData.Instance.GetSceneName(SceneType.RunGameplay2);

        previewedRunMapName = (previewedRunMapName == map1) ? map2 : map1;
        UpdateMapDebugUI();

        if (bossOptions != null && bossOptions.Count > selectedBossIndex && RunManager.Instance != null)
        {
            RunManager.Instance.ConfigureRun(previewedRunMapName, bossOptions[selectedBossIndex].bossPoolType);
        }
    }

    private void UpdateMapDebugUI()
    {
        if (string.IsNullOrEmpty(previewedRunMapName))
        {
            previewedRunMapName = GameSceneData.Instance != null
                ? GameSceneData.Instance.GetRandomRunSceneName()
                : "Run Scene";
        }

        if (txtSelectedMapDebug != null)
        {
            txtSelectedMapDebug.text = $"Map Target: <color=#00FFFF>{previewedRunMapName}</color>";
        }

        Debug.Log($"<color=#FF7700><b>[LobbyBossSelectMenu]</b> Target Map: <b>{previewedRunMapName}</b> (Nhấn 'M' để đổi Map)</color>");
    }

    public void SelectBoss(int index, bool isInitialOpen = false)
    {
        if (bossOptions == null || bossOptions.Count == 0) return;
        if (index < 0 || index >= bossOptions.Count) return;

        var targetOption = bossOptions[index];
        bool isBoss2Unlocked = PlayerPrefs.GetInt("UNLOCKED_BOSS_DARKMAGE", 0) == 1;
        if (targetOption.bossPoolType == PoolType.EnemyDarkMageBoss && !isBoss2Unlocked)
        {
            if (LobbyNotifyManager.Instance != null && !isInitialOpen)
            {
                LobbyNotifyManager.Instance.ShowNotify("Defeat The Earthshaker first to unlock!", Color.red);
            }
            return;
        }

        selectedBossIndex = index;

        for (int i = 0; i < bossOptions.Count; i++)
        {
            var option = bossOptions[i];
            bool isSelected = (i == selectedBossIndex);

            if (option.highlightObject != null)
            {
                option.highlightObject.SetActive(isSelected);
            }
        }

        var selected = bossOptions[selectedBossIndex];
        PlaySelectedBossVideoOnly(selectedBossIndex);

        if (!isInitialOpen && RunManager.Instance != null)
        {
            RunManager.Instance.ConfigureRun(targetRunScene, selected.bossPoolType);
        }

        Debug.Log($"<color=#00FFCC><b>[LobbyBossSelectMenu]</b> Đã chọn: <b>{selected.bossName}</b> | PoolType: <b>{selected.bossPoolType}</b></color>");
        UpdateMapDebugUI();
    }

    private void OnConfirmStartRun()
    {
        if (bossOptions == null || bossOptions.Count == 0) return;

        if (selectedBossIndex < 0 || selectedBossIndex >= bossOptions.Count)
        {
            selectedBossIndex = 0;
        }

        var selected = bossOptions[selectedBossIndex];

        int currentGem = (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
            ? SaveLoadManager.Instance.SaveData.gem
            : 0;

        if (currentGem < selectedWagerAmount)
        {
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify($"Not enough Gems! Required wager: {selectedWagerAmount} Gems.", Color.red);
            }
            return;
        }

        ActiveRunBuffs activeBuffs = new ActiveRunBuffs();

        if (toggleGoldBuff != null && toggleGoldBuff.isOn && toggleGoldBuff.interactable)
        {
            if (InventoryItemManager.Instance != null && InventoryItemManager.Instance.SpendItem(ItemGoldID, 1))
            {
                activeBuffs.hasGoldBuff = true;
                activeBuffs.AddBuff(ItemGoldID);
            }
        }

        if (toggleReviveBuff != null && toggleReviveBuff.isOn && toggleReviveBuff.interactable)
        {
            if (InventoryItemManager.Instance != null && InventoryItemManager.Instance.SpendItem(ItemReviveID, 1))
            {
                activeBuffs.hasReviveBuff = true;
                activeBuffs.AddBuff(ItemReviveID);
            }
        }

        if (toggleAtkBuff != null && toggleAtkBuff.isOn && toggleAtkBuff.interactable)
        {
            if (InventoryItemManager.Instance != null && InventoryItemManager.Instance.SpendItem(ItemAtkID, 1))
            {
                activeBuffs.hasAtkBuff = true;
                activeBuffs.AddBuff(ItemAtkID);
            }
        }

        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            SaveLoadManager.Instance.SaveData.gem -= selectedWagerAmount;
            SaveLoadManager.Instance.SaveGame();
        }

        if (string.IsNullOrEmpty(previewedRunMapName))
        {
            previewedRunMapName = GameSceneData.Instance != null
                ? GameSceneData.Instance.GetRandomRunSceneName()
                : "Run Scene";
        }

        Debug.Log($"<color=#00FF00><b>[START RUN CONFIRMED]</b> Nạp Map: <b>{previewedRunMapName}</b> | Boss Target: <b>{selected.bossName}</b> (PoolType: <b>{selected.bossPoolType}</b>)</color>");

        Close();

        if (RunManager.Instance != null)
        {
            RunManager.Instance.SetWagerConfig(selectedWagerAmount, selectedMultiplier);
            RunManager.Instance.SetActiveBuffs(activeBuffs);
            RunManager.Instance.ConfigureRun(previewedRunMapName, selected.bossPoolType);
            RunManager.Instance.StartRun();
        }
        else
        {
            Debug.LogError("[LobbyBossSelectMenu] Không tìm thấy RunManager để lưu boss đã chọn.");
        }
    }
}
