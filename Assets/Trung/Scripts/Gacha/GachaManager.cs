using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Yêu cầu đã Import DOTween vào Project

public class GachaManager : MonoBehaviour
{
    [Header("Rarity Rate")]
    [SerializeField] [Range(0, 100)] private int commonRate = 60;
    [SerializeField] [Range(0, 100)] private int rareRate = 30;
    [SerializeField] [Range(0, 100)] private int epicRate = 9;
    [SerializeField] [Range(0, 100)] private int legendaryRate = 1;

    [Header("Cost")]
    [SerializeField] private int costRoll1 = 300;
    [SerializeField] private int costRoll5 = 1400;

    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private int lastRollCost;
    [SerializeField] private int lastRollAmount;

    [Header("Card Prefab & Container")]
    [SerializeField] private RuneCardUI cardPrefab;
    [SerializeField] private Transform cardParent;

    [Header("DOTween Reveal Settings (Hiệu ứng bài Huyền Thoại)")]
    [SerializeField] private float legendaryZoomScale = 1.45f;
    [SerializeField] private float legendaryShakeDuration = 0.45f;
    [SerializeField] private float legendaryShakeStrength = 18f;

    [Header("Action Buttons")]
    [SerializeField] private GameObject closeButton;
    [SerializeField] private GameObject rerollButton;
    [SerializeField] private GameObject skipButton; 

    [Header("Main Roll Buttons (Tránh Bug Spam Click)")]
    [SerializeField] private Button roll1Button; 
    [SerializeField] private Button roll5Button; 

    [Header("Inventory Protection Config")]
    [SerializeField] private int maxInventorySlots = 100; 

    [Header("Summoning FX Settings")]
    [SerializeField] private GameObject portalFXRoot;    
    [SerializeField] private Image portalCircleImage;     
    [SerializeField] private Image flashOverlayImage;     
    
    [Header("NEW: Alive Background UI FX (Hiệu ứng UI Nền Sống Động)")]
    [SerializeField] private Image backgroundAuraGlow;    // Ảnh hào quang mờ ảo phía sau Cổng
    [SerializeField] private Transform meteorParticlesParent; // Object cha chứa các tia sáng nền rơi
    [SerializeField] private RectTransform mainCanvasRect; // Kéo thả Canvas Rect vào đây để tính tọa độ rơi
    [SerializeField] private Sprite meteorSprite; // Ô MỚI: Kéo thả ảnh vệt sáng/sao băng vào đây

    [Header("FX Color Themes")]
    [SerializeField] private Color commonColorFX = new Color(1f, 1f, 1f, 0.6f);      
    [SerializeField] private Color rareColorFX = new Color(0.2f, 0.6f, 1f, 0.7f);    
    [SerializeField] private Color epicColorFX = new Color(0.7f, 0.2f, 1f, 0.8f);    
    [SerializeField] private Color legendaryColorFX = new Color(1f, 0.6f, 0f, 0.9f); 
    [Header("Result Cards FX Settings (MỚI)")]
    [SerializeField] private float cardFloatDistance = 12f; // Khoảng cách bài bay lên bay xuống
    [SerializeField] private float cardFloatDuration = 1.8f; // Thời gian chạy 1 chu kỳ bay lơ lửng

    private readonly List<GameObject> currentCards = new List<GameObject>();
    private int revealedCardCount;
    private int totalCardCount;
    
    private Sequence activeLegendarySequence; 
    private Tween auraGlowTween; // Lưu trữ vòng lặp thở của hào quang
    private bool isRollActive = false;

    public static GachaManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        if (skipButton != null) skipButton.SetActive(false);

        // Kích hoạt hiệu ứng "Hào quang thở chậm" lặp vô hạn ngoài sảnh Gacha
        StartBackgroundAuraBreathing();
    }

    private void Update()
    {
        if (isRollActive && Input.GetKeyDown(KeyCode.Escape))
        {
            SkipAllGachaAnimations();
        }
    }

    /// <summary>
    /// HIỆU ỨNG NỀN 1: Tạo chuyển động nảy mờ ảo (Breathing) vô hạn cho hào quang nền
    /// </summary>
    private void StartBackgroundAuraBreathing()
    {
        if (backgroundAuraGlow == null) return;

        // Cho Object nảy nhẹ đều đặn liên tục như đang thở
        backgroundAuraGlow.transform.localScale = Vector3.one;
        auraGlowTween = backgroundAuraGlow.transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 2.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    /// <summary>
    /// HIỆU ỨNG NỀN 2: Sinh ngẫu nhiên các tia sáng sao băng rớt chéo qua màn hình nền sảnh
    /// </summary>
    private void TriggerMeteorStrikeFX(Color themeColor, int count)
    {
        if (meteorParticlesParent == null || mainCanvasRect == null) return;

        for (int i = 0; i < count; i++)
        {
            GameObject meteor = new GameObject("MeteorParticle", typeof(Image));
            meteor.transform.SetParent(meteorParticlesParent, false);
            
            Image img = meteor.GetComponent<Image>();
            
            // === ĐOẠN SỬA ĐỔI: Gán hình ảnh nghệ thuật thay vì để ô vuông trắng ===
            if (meteorSprite != null)
            {
                img.sprite = meteorSprite;
            }
            
            img.color = new Color(themeColor.r, themeColor.g, themeColor.b, Random.Range(0.5f, 0.9f));
            
            // Cấu hình tỷ lệ: Sao băng xịn cần chiều rộng mập hơn một chút để thấy rõ texture ảnh
            RectTransform rect = meteor.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(Random.Range(15f, 35f), Random.Range(150f, 300f)); // Tăng Width lên để ảnh không bị bóp nghẹt
            rect.rotation = Quaternion.Euler(0, 0, 45f); 

            float randomX = Random.Range(-mainCanvasRect.rect.width / 2f, mainCanvasRect.rect.width / 2f);
            float startY = mainCanvasRect.rect.height / 2f + 200f;
            rect.anchoredPosition = new Vector2(randomX, startY);

            float duration = Random.Range(0.6f, 1.2f);
            
            rect.DOAnchorPos(new Vector2(rect.anchoredPosition.x - 500f, -startY), duration).SetEase(Ease.InQuad);
            img.DOFade(0f, duration).SetEase(Ease.InCubic).OnComplete(() => {
                Destroy(meteor);
            });
        }
    }

    #region Roll Logic

    public void Roll1() => Roll(costRoll1, 1);
    public void Roll5() => Roll(costRoll5, 5);

    private void Roll(int cost, int amount)
    {
        if (isRollActive) return; 

        if (RuneInventoryManager.Instance != null)
        {
            int currentRuneCount = RuneInventoryManager.Instance.runes.Count;
            if (currentRuneCount + amount > maxInventorySlots)
            {
                if (LobbyNotifyManager.Instance != null)
                {
                    LobbyNotifyManager.Instance.ShowNotify("Inventory is full! Please dismantle some runes.", Color.red);
                }
                return; 
            }
        }

        if (GemManager.Instance == null || !GemManager.Instance.SpendGem(cost))
        {
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify("Not enough Gems to perform gacha roll!", Color.red);
            }
            return;
        }

        lastRollCost = cost;
        lastRollAmount = amount;

        ClearCards();
        revealedCardCount = 0;
        totalCardCount = amount;
        isRollActive = true;

        if (roll1Button != null) roll1Button.interactable = false;
        if (roll5Button != null) roll5Button.interactable = false;

        if (resultPanel != null) resultPanel.SetActive(false);
        if (closeButton != null) closeButton.SetActive(false);
        if (rerollButton != null) rerollButton.SetActive(false);
        if (skipButton != null) skipButton.SetActive(false); 

        RuneRarity highestRarityInThisRoll = RuneRarity.Common;
        List<RuneData> rolledRunesData = new List<RuneData>();

        for (int i = 0; i < amount; i++)
        {
            RuneData rune = GenerateRandomRune();
            rolledRunesData.Add(rune);

            if (rune.runeRarity > highestRarityInThisRoll)
            {
                highestRarityInThisRoll = rune.runeRarity;
            }

            if (RuneInventoryManager.Instance != null) RuneInventoryManager.Instance.AddRune(rune);

            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.AddRollProgress(1);
                if (rune.runeRarity == RuneRarity.Legendary) AchievementManager.Instance.AddLegendaryProgress(1);
            }
        }

        StartCoroutine(SummoningPortalFXRoutine(highestRarityInThisRoll, rolledRunesData));
    }

    #endregion

    #region Coroutine Portal FX & Spawn Card

    private System.Collections.IEnumerator SummoningPortalFXRoutine(RuneRarity highestRarity, List<RuneData> runesToSpawn)
    {
        if (portalFXRoot != null) portalFXRoot.SetActive(true);

        Color targetFXColor = commonColorFX;
        string rarityAnnounce = "COMMON";
        switch (highestRarity)
        {
            case RuneRarity.Rare: targetFXColor = rareColorFX; rarityAnnounce = "RARE"; break;
            case RuneRarity.Epic: targetFXColor = epicColorFX; rarityAnnounce = "EPIC"; break;
            case RuneRarity.Legendary: targetFXColor = legendaryColorFX; rarityAnnounce = "LEGENDARY"; break;
        }

        if (LobbyNotifyManager.Instance != null)
            LobbyNotifyManager.Instance.ShowNotify($"Sensing {rarityAnnounce} portal energies...", targetFXColor);

        // Đổi màu sắc hào quang nền đồng điệu với độ hiếm cao nhất tức thì
        if (backgroundAuraGlow != null)
        {
            backgroundAuraGlow.DOColor(new Color(targetFXColor.r, targetFXColor.g, targetFXColor.b, 0.4f), 0.5f);
        }

        // Bắn hàng loạt sao băng chéo nền tạo cảm giác bẻ gãy không gian ma pháp
        TriggerMeteorStrikeFX(targetFXColor, runesToSpawn.Count * 6);

        if (portalCircleImage != null)
        {
            portalCircleImage.color = new Color(targetFXColor.r, targetFXColor.g, targetFXColor.b, 0f);
            portalCircleImage.transform.localScale = Vector3.zero;

            portalCircleImage.DOColor(targetFXColor, 0.4f);
            portalCircleImage.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack);
            portalCircleImage.transform.DORotate(new Vector3(0, 0, -1080f), 2.2f, RotateMode.FastBeyond360).SetEase(Ease.InOutCubic);
        }

        yield return new WaitForSeconds(2.0f);

        // HIỆU ỨNG NỀN 3: Rung lắc màn hình (Screen Shake) cực mạnh ngay nhịp lóe sáng nổ bài
        float shakeStrength = highestRarity == RuneRarity.Legendary ? 30f : highestRarity == RuneRarity.Epic ? 15f : 8f;
        Camera.main.transform.DOShakePosition(0.4f, shakeStrength, 25);

        if (flashOverlayImage != null)
        {
            flashOverlayImage.gameObject.SetActive(true);
            flashOverlayImage.color = Color.white;
            flashOverlayImage.DOColor(new Color(1f, 1f, 1f, 0f), 0.5f).OnComplete(() => {
                flashOverlayImage.gameObject.SetActive(false);
            });
        }

        if (portalFXRoot != null) portalFXRoot.SetActive(false);

        if (resultPanel != null) resultPanel.SetActive(true);
        if (skipButton != null) skipButton.SetActive(true);

        if (roll1Button != null) roll1Button.interactable = true;
        if (roll5Button != null) roll5Button.interactable = true;

        // Tiến hành sinh card bài kèm hiệu ứng xuất hiện bắt mắt
        for (int i = 0; i < runesToSpawn.Count; i++)
        {
            RuneCardUI card = SpawnCard(runesToSpawn[i]);
            
            if (card != null)
            {
                // 1. Hiệu ứng xuất hiện: Zoom lớn từ tâm ra kèm giật nảy Punch Scale
                card.transform.localScale = Vector3.zero;
                card.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
                
                // Tạo một chuỗi Sequence để quản lý nhịp chuyển động liên tục
                Sequence cardAppearSeq = DOTween.Sequence();
                cardAppearSeq.SetDelay(0.1f * i);
                cardAppearSeq.Append(card.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), 0.25f, 10, 1f));
                
                // 2. HIỆU ỨNG MỚI BẮT MẮT: Sau khi nổ xuất hiện xong, bài tự động chuyển sang trạng thái bay lơ lửng vô hạn (Idle Float)
                cardAppearSeq.OnComplete(() => {
                    if (card != null)
                    {
                        // Cho lá bài di chuyển lên xuống nhẹ nhàng tạo độ sống động
                        card.transform.DOLocalMoveY(card.transform.localPosition.y + cardFloatDistance, cardFloatDuration)
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetEase(Ease.InOutSine);
                            
                        // Thêm một chút hiệu ứng nghiêng góc nhẹ (Rotate) ngẫu nhiên để bài nhìn tự nhiên hơn
                        card.transform.DORotate(new Vector3(0, 0, Random.Range(-2f, 2f)), cardFloatDuration * 1.2f)
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetEase(Ease.InOutSine);
                    }
                });
            }
            
            // Đợi gối đầu một chút tạo nhịp rớt bài tuần tự đẹp mắt
            yield return new WaitForSeconds(0.08f);
        }
        if (InventoryUI.Instance != null && InventoryUI.Instance.gameObject.activeInHierarchy)
        {
            InventoryUI.Instance.RefreshInventory();
        }
    }

    #endregion

    #region DOTween Reveal Animation

    public void TriggerLegendaryRevealAction(RuneCardUI legendaryCard)
    {
        if (legendaryCard == null || cardParent == null || !isRollActive) return;

        List<CanvasGroup> otherGroups = new List<CanvasGroup>();
        RuneCardUI[] allCards = cardParent.GetComponentsInChildren<RuneCardUI>();
        
        foreach (var card in allCards)
        {
            if (card == legendaryCard) continue;
            CanvasGroup group = card.GetComponent<CanvasGroup>() ?? card.gameObject.AddComponent<CanvasGroup>();
            otherGroups.Add(group);
            group.DOFade(0f, 0.25f); 
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        RectTransform legendRect = legendaryCard.transform as RectTransform;
        Vector3 originalScale = legendRect.localScale;
        Vector3 originalPos = legendRect.localPosition;

        activeLegendarySequence = DOTween.Sequence();
        activeLegendarySequence.Append(legendRect.DOShakePosition(legendaryShakeDuration, legendaryShakeStrength, 30));

        Vector3 targetScale = originalScale * legendaryZoomScale;
        activeLegendarySequence.Append(legendRect.DOScale(targetScale, 0.35f).SetEase(Ease.OutCubic));
        activeLegendarySequence.Join(legendRect.DOMove(cardParent.position, 0.35f).SetEase(Ease.OutCubic)); 

        activeLegendarySequence.AppendInterval(0.15f);
        activeLegendarySequence.Append(legendRect.DOPunchScale(new Vector3(0.12f, 0.12f, 0.12f), 0.2f, 5, 0.5f)); 
        activeLegendarySequence.AppendCallback(() => 
        {
            legendaryCard.StartInternalReveal();
        });

        activeLegendarySequence.AppendInterval(0.6f);

        activeLegendarySequence.Append(legendRect.DOScale(originalScale, 0.25f).SetEase(Ease.InCubic));
        activeLegendarySequence.Join(legendRect.DOLocalMove(originalPos, 0.25f).SetEase(Ease.InCubic));

        activeLegendarySequence.OnComplete(() =>
        {
            foreach (var group in otherGroups)
            {
                if (group != null)
                {
                    group.DOFade(1f, 0.2f);
                    group.blocksRaycasts = true;
                    group.interactable = true;
                }
            }
            activeLegendarySequence = null;
        });
    }

    #endregion

    #region Logic Skip Bỏ Qua Hoạt Cảnh

    public void SkipAllGachaAnimations()
    {
        if (!isRollActive) return;

        if (activeLegendarySequence != null)
        {
            activeLegendarySequence.Kill(true); 
            activeLegendarySequence = null;
        }

        RuneCardUI[] allCards = cardParent.GetComponentsInChildren<RuneCardUI>();
        foreach (RuneCardUI card in allCards)
        {
            if (card == null) continue;

            card.transform.DOKill(true); 

            if (!card.IsRevealed())
            {
                card.ForceReveal(); 
            }
        }

        revealedCardCount = totalCardCount;
        isRollActive = false;

        if (closeButton != null) closeButton.SetActive(true);
        if (rerollButton != null) rerollButton.SetActive(true);
        if (skipButton != null) skipButton.SetActive(false); 

        if (LobbyNotifyManager.Instance != null)
        {
            LobbyNotifyManager.Instance.ShowNotify("Animations skipped. Runes added to vault.", Color.white);
        }
    }

    #endregion

    #region Spawn & Helper Logic

    private RuneCardUI SpawnCard(RuneData runeData)
    {
        RuneCardUI card = Instantiate(cardPrefab, cardParent);
        card.Setup(runeData);
        currentCards.Add(card.gameObject);
        return card;
    }

    private void ClearCards()
    {
        currentCards.Clear();

        if (cardParent != null)
        {
            // CHÈN THÊM DÒNG NÀY: Diệt sạch toàn bộ các hiệu ứng bay lơ lửng cũ trên Content Grid
            cardParent.DOKill();

            for (int i = cardParent.childCount - 1; i >= 0; i--)
            {
                if (cardParent.GetChild(i) != null)
                {
                    // Diệt hiệu ứng của từng lá bài cũ trước khi hủy Object vật lý
                    cardParent.GetChild(i).DOKill();
                    Destroy(cardParent.GetChild(i).gameObject);
                }
            }
        }

        if (backgroundAuraGlow != null)
        {
            backgroundAuraGlow.DOColor(new Color(1f, 1f, 1f, 0.2f), 0.4f);
        }
    }

    public void NotifyCardRevealed()
    {
        if (!isRollActive) return;

        revealedCardCount++;
        if (revealedCardCount < totalCardCount) return;

        isRollActive = false;
        if (closeButton != null) closeButton.SetActive(true);
        if (rerollButton != null) rerollButton.SetActive(true);
        if (skipButton != null) skipButton.SetActive(false);

        if (LobbyNotifyManager.Instance != null)
        {
            LobbyNotifyManager.Instance.ShowNotify("All runes successfully summoned!", Color.green);
        }
    }

    public void CloseResultPanel()
    {
        isRollActive = false;
        if (resultPanel != null) resultPanel.SetActive(false);
        ClearCards();
    }

    public void ReRoll()
    {
        if (lastRollAmount <= 0) return;
        Roll(lastRollCost, lastRollAmount);
    }

    private void OnDestroy()
    {
        if (auraGlowTween != null) auraGlowTween.Kill();
    }

    #endregion

    #region Generate Data Helpers

    private RuneData GenerateRandomRune()
    {
        RuneColor runeColor = RandomRuneColor();
        RuneRarity runeRarity = RandomRuneRarity();
        RuneData rune = new RuneData(runeColor, runeRarity);
        AssignRuneLore(rune);
        GenerateAffixes(rune);
        return rune;
    }

    private RuneColor RandomRuneColor()
    {
        int random = Random.Range(0, 100);
        if (random < 30) return RuneColor.Red;
        if (random < 65) return RuneColor.Green;
        return RuneColor.Blue;
    }

    private RuneRarity RandomRuneRarity()
    {
        int totalRate = commonRate + rareRate + epicRate + legendaryRate;
        if (totalRate <= 0) return RuneRarity.Common;

        int random = Random.Range(0, totalRate);
        if (random < commonRate) return RuneRarity.Common;
        random -= commonRate;
        if (random < rareRate) return RuneRarity.Rare;
        random -= rareRate;
        if (random < epicRate) return RuneRarity.Epic;
        return RuneRarity.Legendary;
    }

    private void GenerateAffixes(RuneData rune)
    {
        int affixCount = GetAffixCount(rune.runeRarity);
        List<RuneStatType> usedStats = new List<RuneStatType>();

        for (int i = 0; i < affixCount; i++)
        {
            RuneStatType statType = GetRandomStat(usedStats);
            usedStats.Add(statType);
            float value = GetRandomValue(statType, rune.runeRarity);
            rune.affixes.Add(new RuneAffixData(statType, value));
        }
    }

    private int GetAffixCount(RuneRarity runeRarity)
    {
        switch (runeRarity)
        {
            case RuneRarity.Common: return 1;
            case RuneRarity.Rare: return 2;
            case RuneRarity.Epic: return 3;
            case RuneRarity.Legendary: return 4;
        }
        return 1;
    }

    private RuneStatType GetRandomStat(List<RuneStatType> usedStats)
    {
        List<RuneStatType> pool = new List<RuneStatType>()
        {
            RuneStatType.HP, RuneStatType.HPPercent, RuneStatType.MP, RuneStatType.MPPercent,
            RuneStatType.Stamina, RuneStatType.StaminaPercent, RuneStatType.ATK, RuneStatType.ATKPercent,
            RuneStatType.DEF, RuneStatType.DEFPercent, RuneStatType.CritChance, RuneStatType.CritDamage,
            RuneStatType.ArmorPenetration, RuneStatType.StaminaRegen
        };

        for (int i = pool.Count - 1; i >= 0; i--)
        {
            if (usedStats.Contains(pool[i])) pool.RemoveAt(i);
        }
        return pool[Random.Range(0, pool.Count)];
    }

    private float GetRandomValue(RuneStatType statType, RuneRarity rarity)
    {
        switch (statType)
        {
            case RuneStatType.HP: return GetValueByRarity(rarity, 80f, 180f, 180f, 350f, 350f, 650f, 650f, 1200f);
            case RuneStatType.MP: return GetValueByRarity(rarity, 25f, 60f, 60f, 120f, 120f, 220f, 220f, 400f);
            case RuneStatType.Stamina: return GetValueByRarity(rarity, 15f, 40f, 40f, 80f, 80f, 140f, 140f, 250f);
            case RuneStatType.ATK: return GetValueByRarity(rarity, 3f, 8f, 8f, 18f, 18f, 35f, 35f, 60f);
            case RuneStatType.DEF: return GetValueByRarity(rarity, 2f, 6f, 6f, 14f, 14f, 28f, 28f, 50f);
            case RuneStatType.HPPercent: return GetValueByRarity(rarity, 2f, 4f, 4f, 7f, 7f, 12f, 12f, 20f);
            case RuneStatType.MPPercent: return GetValueByRarity(rarity, 2f, 4f, 4f, 7f, 7f, 12f, 12f, 18f);
            case RuneStatType.StaminaPercent: return GetValueByRarity(rarity, 3f, 5f, 5f, 9f, 9f, 15f, 15f, 25f);
            case RuneStatType.ATKPercent: return GetValueByRarity(rarity, 2f, 4f, 4f, 7f, 7f, 12f, 12f, 18f);
            case RuneStatType.DEFPercent: return GetValueByRarity(rarity, 2f, 4f, 4f, 7f, 7f, 12f, 12f, 18f);
            case RuneStatType.CritChance: return GetValueByRarity(rarity, 1f, 3f, 3f, 6f, 6f, 10f, 10f, 18f);
            case RuneStatType.CritDamage: return GetValueByRarity(rarity, 4f, 8f, 8f, 15f, 15f, 25f, 25f, 40f);
            case RuneStatType.ArmorPenetration: return GetValueByRarity(rarity, 2f, 5f, 5f, 9f, 9f, 15f, 15f, 25f);
            case RuneStatType.StaminaRegen: return GetValueByRarity(rarity, 3f, 6f, 6f, 10f, 10f, 18f, 18f, 30f);
        }
        return 1f;
    }

    private float GetValueByRarity(RuneRarity rarity, float cMin, float cMax, float rMin, float rMax, float eMin, float eMax, float lMin, float lMax)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return Random.Range(cMin, cMax);
            case RuneRarity.Rare: return Random.Range(rMin, rMax);
            case RuneRarity.Epic: return Random.Range(eMin, eMax);
            case RuneRarity.Legendary: return Random.Range(lMin, lMax);
        }
        return 1f;
    }

    private void AssignRuneLore(RuneData rune)
    {
        switch (rune.runeColor)
        {
            case RuneColor.Red: AssignRedLore(rune); break;
            case RuneColor.Green: AssignGreenLore(rune); break;
            case RuneColor.Blue: AssignBlueLore(rune); break;
        }
    }

    private void AssignRedLore(RuneData rune)
    {
        switch (rune.runeRarity)
        {
            case RuneRarity.Common: rune.runeName = "Ashfang"; rune.runeLore = "Its heat faded long ago, yet the scar remains."; break;
            case RuneRarity.Rare: rune.runeName = "Blood Oath"; rune.runeLore = "The knight survived the battle. His comrades did not."; break;
            case RuneRarity.Epic: rune.runeName = "Heart of Ruin"; rune.runeLore = "Every beat echoed like a war drum beneath the earth."; break;
            case RuneRarity.Legendary: rune.runeName = "Crimson Crown"; rune.runeLore = "Kings burned kingdoms to wear it for a single night."; break;
        }
    }

    private void AssignGreenLore(RuneData rune)
    {
        switch (rune.runeRarity)
        {
            case RuneRarity.Common: rune.runeName = "Wiltroot"; rune.runeLore = "It grew where no light should ever reach."; break;
            case RuneRarity.Rare: rune.runeName = "Verdant Pulse"; rune.runeLore = "The forest whispered back when spoken to."; break;
            case RuneRarity.Epic: rune.runeName = "Hollow Bloom"; rune.runeLore = "Flowers fed on the dead beneath the ruins."; break;
            case RuneRarity.Legendary: rune.runeName = "Worldsap Core"; rune.runeLore = "Its roots once held an entire civilization together."; break;
        }
    }

    private void AssignBlueLore(RuneData rune)
    {
        switch (rune.runeRarity)
        {
            case RuneRarity.Common: rune.runeName = "Frost Vein"; rune.runeLore = "Cold enough to silence fear itself."; break;
            case RuneRarity.Rare: rune.runeName = "Moon Shard"; rune.runeLore = "Fragments of a sky long forgotten."; break;
            case RuneRarity.Epic: rune.runeName = "Deep Current"; rune.runeLore = "Something ancient moved beneath the tide."; break;
            case RuneRarity.Legendary: rune.runeName = "Eye of Eternity"; rune.runeLore = "It watched the end before time understood death."; break;
        }
    }

    #endregion
}