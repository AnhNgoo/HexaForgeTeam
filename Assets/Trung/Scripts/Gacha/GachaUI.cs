using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GachaUI : MonoBehaviour
{
    public static GachaUI Instance;

    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;

    [Header("Card Prefab & Container")]
    [SerializeField] private RuneCardUI cardPrefab;
    [SerializeField] private Transform cardParent;

    [Header("DOTween Reveal Settings")]
    [SerializeField] private float legendaryZoomScale = 1.45f;
    [SerializeField] private float legendaryShakeDuration = 0.45f;
    [SerializeField] private float legendaryShakeStrength = 18f;

    [Header("Action Buttons")]
    [SerializeField] private GameObject closeButton;
    [SerializeField] private GameObject rerollButton;
    [SerializeField] private GameObject skipButton;

    [Header("Main Roll Buttons & Cost Displays")]
    [SerializeField] private Button roll1Button;
    [SerializeField] private Button roll10Button;
    [SerializeField] private CostDisplayUI cost1DisplayUI;
    [SerializeField] private CostDisplayUI cost10DisplayUI;

    [Header("Summoning FX Settings")]
    [SerializeField] private GameObject portalFXRoot;
    [SerializeField] private Image portalCircleImage;
    [SerializeField] private Image flashOverlayImage;

    [Header("Alive Background UI FX")]
    [SerializeField] private Image backgroundAuraGlow;
    [SerializeField] private Transform meteorParticlesParent;
    [SerializeField] private RectTransform mainCanvasRect;
    [SerializeField] private Sprite meteorSprite;

    [Header("FX Color Themes")]
    [SerializeField] private Color commonColorFX = new Color(1f, 1f, 1f, 0.6f);
    [SerializeField] private Color rareColorFX = new Color(0.2f, 0.6f, 1f, 0.7f);
    [SerializeField] private Color epicColorFX = new Color(0.7f, 0.2f, 1f, 0.8f);
    [SerializeField] private Color legendaryColorFX = new Color(1f, 0.6f, 0f, 0.9f);

    private readonly List<GameObject> currentCards = new List<GameObject>();
    private List<RuneData> cachedPendingRunes = new List<RuneData>();
    private Sequence activeLegendarySequence;
    private Tween auraGlowTween;
    private Coroutine summoningRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        if (skipButton != null) skipButton.SetActive(false);

        StartBackgroundAuraBreathing();
    }

    private void OnEnable()
    {
        RefreshCostUI();
    }

    public bool IsResultPanelActive()
    {
        return resultPanel != null && resultPanel.activeInHierarchy;
    }

    public void RefreshCostUI()
    {
        int ownedTickets = 0;
        if (InventoryItemManager.Instance != null)
        {
            ownedTickets = InventoryItemManager.Instance.GetItemQuantity("GACHA_TICKET_01");
        }

        if (cost1DisplayUI != null)
        {
            List<CostData> costs1 = new List<CostData>();
            if (ownedTickets >= 1)
            {
                costs1.Add(new CostData("GACHA_TICKET_01", 1));
            }
            else
            {
                costs1.Add(new CostData("GEM", 120));
            }
            cost1DisplayUI.SetupCost(costs1);
        }

        if (cost10DisplayUI != null)
        {
            List<CostData> costs10 = new List<CostData>();
            int ticketsToUse = Mathf.Min(ownedTickets, 10);
            int missingRolls = 10 - ticketsToUse;

            if (ticketsToUse > 0)
            {
                costs10.Add(new CostData("GACHA_TICKET_01", ticketsToUse));
            }

            if (missingRolls > 0)
            {
                int gemNeeded = (ticketsToUse == 0) ? 1080 : (missingRolls * 120);
                costs10.Add(new CostData("GEM", gemNeeded));
            }

            cost10DisplayUI.SetupCost(costs10);
        }
    }

    private void StartBackgroundAuraBreathing()
    {
        if (backgroundAuraGlow == null) return;

        backgroundAuraGlow.transform.localScale = Vector3.one;
        auraGlowTween = backgroundAuraGlow.transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 2.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void TriggerMeteorStrikeFX(Color themeColor, int count)
    {
        if (meteorParticlesParent == null || mainCanvasRect == null) return;

        for (int i = 0; i < count; i++)
        {
            GameObject meteor = new GameObject("MeteorParticle", typeof(Image));
            meteor.transform.SetParent(meteorParticlesParent, false);

            Image img = meteor.GetComponent<Image>();

            if (meteorSprite != null)
            {
                img.sprite = meteorSprite;
            }

            img.color = new Color(themeColor.r, themeColor.g, themeColor.b, Random.Range(0.5f, 0.9f));

            RectTransform rect = meteor.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(Random.Range(15f, 35f), Random.Range(150f, 300f));
            rect.rotation = Quaternion.Euler(0, 0, 45f);

            float randomX = Random.Range(-mainCanvasRect.rect.width / 2f, mainCanvasRect.rect.width / 2f);
            float startY = mainCanvasRect.rect.height / 2f + 200f;
            rect.anchoredPosition = Vector2.zero + new Vector2(randomX, startY);

            float duration = Random.Range(0.6f, 1.2f);

            rect.DOAnchorPos(new Vector2(rect.anchoredPosition.x - 500f, -startY), duration).SetEase(Ease.InQuad);
            img.DOFade(0f, duration).SetEase(Ease.InCubic).OnComplete(() => {
                Destroy(meteor);
            });
        }
    }

    public void PlaySummoningFX(RuneRarity highestRarity, List<RuneData> runesToSpawn)
    {
        cachedPendingRunes = new List<RuneData>(runesToSpawn);
        if (summoningRoutine != null) StopCoroutine(summoningRoutine);
        summoningRoutine = StartCoroutine(SummoningPortalFXRoutine(highestRarity, runesToSpawn));
    }

    public void StopAllSummoningCoroutines()
    {
        if (summoningRoutine != null)
        {
            StopCoroutine(summoningRoutine);
            summoningRoutine = null;
        }

        if (portalFXRoot != null) portalFXRoot.SetActive(false);
        if (flashOverlayImage != null) flashOverlayImage.gameObject.SetActive(false);
    }

    private IEnumerator SummoningPortalFXRoutine(RuneRarity highestRarity, List<RuneData> runesToSpawn)
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

        if (backgroundAuraGlow != null)
        {
            backgroundAuraGlow.DOColor(new Color(targetFXColor.r, targetFXColor.g, targetFXColor.b, 0.4f), 0.5f);
        }

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

        float shakeStrength = highestRarity == RuneRarity.Legendary ? 30f : highestRarity == RuneRarity.Epic ? 15f : 8f;
        if (Camera.main != null) Camera.main.transform.DOShakePosition(0.4f, shakeStrength, 25);

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

        SetMainRollButtonsInteractable(true);

        float targetCardScale = runesToSpawn.Count > 5 ? 0.9f : 0.85f;

        for (int i = 0; i < runesToSpawn.Count; i++)
        {
            RuneCardUI card = SpawnCard(runesToSpawn[i]);

            if (card != null)
            {
                card.transform.rotation = Quaternion.identity;
                card.transform.localScale = Vector3.zero;

                card.transform.DOScale(Vector3.one * targetCardScale, 0.35f).SetEase(Ease.OutBack);

                Sequence cardAppearSeq = DOTween.Sequence();
                cardAppearSeq.SetDelay(0.06f * i);
                cardAppearSeq.Append(card.transform.DOPunchScale(new Vector3(0.08f, 0.08f, 0.08f), 0.2f, 8, 0.8f));
            }

            yield return new WaitForSeconds(0.06f);
        }

        if (RuneInventoryUI.Instance != null && RuneInventoryUI.Instance.gameObject.activeInHierarchy)
        {
            RuneInventoryUI.Instance.RefreshInventory();
        }

        RefreshCostUI();
    }

    private RuneCardUI SpawnCard(RuneData runeData)
    {
        RuneCardUI card = Instantiate(cardPrefab, cardParent);
        card.Setup(runeData);
        currentCards.Add(card.gameObject);
        return card;
    }

    public void ClearCards()
    {
        currentCards.Clear();

        if (cardParent != null)
        {
            cardParent.DOKill();

            for (int i = cardParent.childCount - 1; i >= 0; i--)
            {
                if (cardParent.GetChild(i) != null)
                {
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

    public void TriggerLegendaryRevealAction(RuneCardUI legendaryCard)
    {
        if (legendaryCard == null || cardParent == null) return;

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
        activeLegendarySequence.AppendCallback(() => {
            legendaryCard.StartInternalReveal();
        });

        activeLegendarySequence.AppendInterval(0.6f);

        activeLegendarySequence.Append(legendRect.DOScale(originalScale, 0.25f).SetEase(Ease.InCubic));
        activeLegendarySequence.Join(legendRect.DOLocalMove(originalPos, 0.25f).SetEase(Ease.InCubic));

        activeLegendarySequence.OnComplete(() => {
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

    public void ForceInstantRevealAll()
    {
        StopAllSummoningCoroutines();

        if (activeLegendarySequence != null)
        {
            activeLegendarySequence.Kill(false);
            activeLegendarySequence = null;
        }

        if (portalFXRoot != null) portalFXRoot.SetActive(false);
        if (flashOverlayImage != null) flashOverlayImage.gameObject.SetActive(false);

        if (cardParent == null) return;

        ClearCards();

        if (cachedPendingRunes != null && cachedPendingRunes.Count > 0)
        {
            float targetCardScale = cachedPendingRunes.Count > 5 ? 0.9f : 0.85f;

            for (int i = 0; i < cachedPendingRunes.Count; i++)
            {
                RuneCardUI card = SpawnCard(cachedPendingRunes[i]);
                if (card != null)
                {
                    card.transform.DOKill();
                    card.transform.rotation = Quaternion.identity;
                    card.transform.localScale = Vector3.one * targetCardScale;

                    CanvasGroup group = card.GetComponent<CanvasGroup>();
                    if (group != null)
                    {
                        group.DOKill();
                        group.alpha = 1f;
                        group.blocksRaycasts = true;
                        group.interactable = true;
                    }

                    card.InstantRevealWithoutAnimation();
                }
            }
        }

        SetMainRollButtonsInteractable(true);
        if (resultPanel != null) resultPanel.SetActive(true);

        if (RuneInventoryUI.Instance != null && RuneInventoryUI.Instance.gameObject.activeInHierarchy)
        {
            RuneInventoryUI.Instance.RefreshInventory();
        }

        RefreshCostUI();
    }

    public void SetMainRollButtonsInteractable(bool interactable)
    {
        if (roll1Button != null) roll1Button.interactable = interactable;
        if (roll10Button != null) roll10Button.interactable = interactable;
    }

    public void ToggleUIPanels(bool isRolling)
    {
        if (resultPanel != null) resultPanel.SetActive(!isRolling);
        if (closeButton != null) closeButton.SetActive(!isRolling);
        if (rerollButton != null) rerollButton.SetActive(!isRolling);
        if (skipButton != null) skipButton.SetActive(isRolling);
    }

    public void SetSkipButtonActive(bool active)
    {
        if (skipButton != null) skipButton.SetActive(active);
    }

    public void SetResultPanelActive(bool active)
    {
        if (resultPanel != null) resultPanel.SetActive(active);
    }

    private void OnDestroy()
    {
        if (auraGlowTween != null) auraGlowTween.Kill();
    }
}