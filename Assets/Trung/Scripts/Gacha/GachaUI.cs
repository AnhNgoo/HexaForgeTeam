using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using DG.Tweening;

public class GachaUI : MonoBehaviour
{
    public static GachaUI Instance;

    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private CanvasGroup resultCanvasGroup;

    [Header("Result Video Player Settings")]
    [SerializeField] private VideoPlayer resultVideoPlayer;
    [SerializeField] private RawImage resultVideoRawImage;
    [SerializeField] private RenderTexture resultVideoRenderTexture;

    [Header("Card Prefab & Container")]
    [SerializeField] private RuneCardUI cardPrefab;
    [SerializeField] private Transform cardParent;

    [Header("Smooth Reveal Settings")]
    [SerializeField] private float cardPopDuration = 0.35f;
    [SerializeField] private float legendaryScaleMultiplier = 1.25f;

    [Header("Action Buttons")]
    [SerializeField] private GameObject closeButton;
    [SerializeField] private GameObject rerollButton;
    [SerializeField] private GameObject skipButton;

    [Header("Info / Rate Dialog Settings")]
    [SerializeField] private Button infoButton;
    [SerializeField] private GameObject infoPanelRoot;
    [SerializeField] private CanvasGroup infoCanvasGroup;
    [SerializeField] private TMP_Text infoRatesText;
    [SerializeField] private Button infoCloseButton;

    [Header("Main Roll Buttons & Cost Displays")]
    [SerializeField] private Button roll1Button;
    [SerializeField] private Button roll10Button;
    [SerializeField] private CostDisplayUI cost1DisplayUI;
    [SerializeField] private CostDisplayUI cost10DisplayUI;

    [Header("Gacha Video Player Settings")]
    [SerializeField] private GameObject videoPanelRoot;
    [SerializeField] private CanvasGroup videoCanvasGroup;
    [SerializeField] private VideoPlayer gachaVideoPlayer;
    [SerializeField] private RawImage videoRawImage;
    [SerializeField] private RenderTexture videoRenderTexture;

    private readonly List<GameObject> currentCards = new List<GameObject>();
    private List<RuneData> cachedPendingRunes = new List<RuneData>();
    private Sequence activeLegendarySequence;
    private Coroutine videoPlayRoutine;
    private Coroutine cardSpawnRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (resultPanel != null)
        {
            if (resultCanvasGroup == null) resultCanvasGroup = resultPanel.GetComponent<CanvasGroup>() ?? resultPanel.AddComponent<CanvasGroup>();
            resultPanel.SetActive(false);
        }

        if (videoPanelRoot != null)
        {
            if (videoCanvasGroup == null) videoCanvasGroup = videoPanelRoot.GetComponent<CanvasGroup>() ?? videoPanelRoot.AddComponent<CanvasGroup>();
            videoPanelRoot.SetActive(false);
        }

        if (infoPanelRoot != null)
        {
            if (infoCanvasGroup == null) infoCanvasGroup = infoPanelRoot.GetComponent<CanvasGroup>() ?? infoPanelRoot.AddComponent<CanvasGroup>();
            infoPanelRoot.SetActive(false);
        }

        if (skipButton != null) skipButton.SetActive(false);

        if (gachaVideoPlayer != null)
        {
            gachaVideoPlayer.loopPointReached += OnVideoEndReached;
        }

        if (infoButton != null)
        {
            infoButton.onClick.RemoveAllListeners();
            infoButton.onClick.AddListener(() =>
            {
                AnimateButtonPunch(infoButton.transform);
                OpenInfoPanel();
            });
        }

        if (infoCloseButton != null)
        {
            infoCloseButton.onClick.RemoveAllListeners();
            infoCloseButton.onClick.AddListener(CloseInfoPanel);
        }

        if (skipButton != null)
        {
            Button btn = skipButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (GachaManager.Instance != null)
                    {
                        GachaManager.Instance.SkipAllGachaAnimations();
                    }
                });
            }
        }
    }

    private void Update()
    {
        if (infoPanelRoot != null && infoPanelRoot.activeInHierarchy && Input.GetMouseButtonDown(0))
        {
            RectTransform rect = infoPanelRoot.GetComponent<RectTransform>();
            if (rect != null && !RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, null))
            {
                CloseInfoPanel();
            }
        }
    }

    private void OnEnable()
    {
        RefreshCostUI();
    }

    public void OpenInfoPanel()
    {
        if (infoPanelRoot == null) return;

        if (infoRatesText != null)
        {
            infoRatesText.text = 
                "<color=#FFCC00><b>SUMMON RATES & INFO</b></color>\n\n" +
                "<color=#FFFFFF>• Common:</color> <color=#00FFCC><b>65%</b></color> (1 Affix)\n" +
                "<color=#3399FF>• Rare:</color> <color=#00FFCC><b>25%</b></color> (2 Affixes)\n" +
                "<color=#B266FF>• Epic:</color> <color=#00FFCC><b>8%</b></color> (3 Affixes)\n" +
                "<color=#FF9900>• Legendary:</color> <color=#00FFCC><b>2%</b></color> (4 Affixes)\n\n" +
                "<i>Duplicated runes can be dismantled for Shards & Gems.</i>";
        }

        infoPanelRoot.SetActive(true);

        if (infoCanvasGroup != null)
        {
            infoCanvasGroup.DOKill();
            infoCanvasGroup.alpha = 0f;
            infoCanvasGroup.DOFade(1f, 0.2f).SetUpdate(true);
        }

        Transform panelContent = infoPanelRoot.transform;
        panelContent.DOKill(true);
        panelContent.localScale = Vector3.one * 0.8f;
        panelContent.DOScale(Vector3.one, 0.22f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void CloseInfoPanel()
    {
        if (infoPanelRoot == null || !infoPanelRoot.activeInHierarchy) return;

        if (infoCanvasGroup != null)
        {
            infoCanvasGroup.DOKill();
            infoCanvasGroup.DOFade(0f, 0.15f).SetUpdate(true).OnComplete(() =>
            {
                infoPanelRoot.SetActive(false);
            });
        }
        else
        {
            infoPanelRoot.SetActive(false);
        }
    }

    private void AnimateButtonPunch(Transform btnTransform)
    {
        if (btnTransform == null) return;
        btnTransform.DOKill(true);
        btnTransform.localScale = Vector3.one;
        btnTransform.DOPunchScale(new Vector3(0.08f, 0.08f, 0f), 0.2f, 5, 0.5f).SetUpdate(true);
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

    public void PlaySummoningFX(RuneRarity highestRarity, List<RuneData> runesToSpawn)
    {
        cachedPendingRunes = new List<RuneData>(runesToSpawn);

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.gameObject.SetActive(false);
        }

        CloseInfoPanel();
        StopAllSummoningCoroutines();
        videoPlayRoutine = StartCoroutine(PlayGachaVideoRoutine());
    }

    private IEnumerator PlayGachaVideoRoutine()
    {
        if (videoPanelRoot != null)
        {
            videoPanelRoot.SetActive(true);
            if (videoCanvasGroup != null)
            {
                videoCanvasGroup.DOKill();
                videoCanvasGroup.alpha = 0f;
                videoCanvasGroup.DOFade(1f, 0.2f).SetUpdate(true);
            }
        }

        if (skipButton != null) skipButton.SetActive(true);

        if (gachaVideoPlayer != null)
        {
            if (videoRenderTexture != null)
            {
                videoRenderTexture.Release();
            }

            gachaVideoPlayer.Stop();
            gachaVideoPlayer.Prepare();

            while (!gachaVideoPlayer.isPrepared)
            {
                yield return null;
            }

            gachaVideoPlayer.Play();
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
            OnVideoEndReached(null);
        }
    }

    private void OnVideoEndReached(VideoPlayer source)
    {
        StopGachaVideoOnly();
        cardSpawnRoutine = StartCoroutine(SpawnAndAnimateCardsRoutine());
    }

    private IEnumerator SpawnAndAnimateCardsRoutine()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            if (resultCanvasGroup != null)
            {
                resultCanvasGroup.DOKill();
                resultCanvasGroup.alpha = 0f;
                resultCanvasGroup.DOFade(1f, 0.25f).SetUpdate(true);
            }
        }

        // Kích hoạt phát video Result khi panel kết quả hiện lên
        PlayResultVideo();

        if (skipButton != null) skipButton.SetActive(true);

        ClearCards();

        float targetCardScale = cachedPendingRunes.Count > 5 ? 0.9f : 0.85f;

        for (int i = 0; i < cachedPendingRunes.Count; i++)
        {
            RuneCardUI card = SpawnCard(cachedPendingRunes[i]);

            if (card != null)
            {
                card.transform.rotation = Quaternion.identity;
                card.transform.localScale = Vector3.zero;

                card.transform.DOScale(Vector3.one * targetCardScale, cardPopDuration)
                    .SetEase(Ease.OutBack, 1.1f);
            }

            yield return new WaitForSeconds(0.08f);
        }

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.gameObject.SetActive(true);
            LobbyHUDTopBar.Instance.RefreshCurrencyUI();
        }

        SetMainRollButtonsInteractable(true);

        if (RuneInventoryUI.Instance != null && RuneInventoryUI.Instance.gameObject.activeInHierarchy)
        {
            RuneInventoryUI.Instance.RefreshInventory();
        }

        RefreshCostUI();
    }

    private void PlayResultVideo()
    {
        if (resultVideoPlayer != null)
        {
            if (resultVideoRenderTexture != null)
            {
                resultVideoRenderTexture.Release();
            }
            resultVideoPlayer.Stop();
            resultVideoPlayer.Prepare();
            resultVideoPlayer.Play();
        }
    }

    private void StopResultVideo()
    {
        if (resultVideoPlayer != null)
        {
            resultVideoPlayer.Stop();
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
            group.DOFade(0.2f, 0.3f);
        }

        RectTransform legendRect = legendaryCard.transform as RectTransform;
        Vector3 originalScale = legendRect.localScale;
        Vector3 originalPos = legendRect.localPosition;

        activeLegendarySequence = DOTween.Sequence();
        
        Vector3 targetScale = originalScale * legendaryScaleMultiplier;
        activeLegendarySequence.Append(legendRect.DOScale(targetScale, 0.35f).SetEase(Ease.OutCubic));
        activeLegendarySequence.Join(legendRect.DOMove(cardParent.position, 0.35f).SetEase(Ease.OutCubic));

        activeLegendarySequence.Append(legendRect.DOPunchScale(new Vector3(0.06f, 0.06f, 0f), 0.25f, 4, 0.4f));
        activeLegendarySequence.AppendCallback(() => {
            legendaryCard.StartInternalReveal();
        });

        activeLegendarySequence.AppendInterval(0.5f);

        activeLegendarySequence.Append(legendRect.DOScale(originalScale, 0.3f).SetEase(Ease.InOutCubic));
        activeLegendarySequence.Join(legendRect.DOLocalMove(originalPos, 0.3f).SetEase(Ease.InOutCubic));

        activeLegendarySequence.OnComplete(() => {
            foreach (var group in otherGroups)
            {
                if (group != null)
                {
                    group.DOFade(1f, 0.25f);
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
                    }

                    card.InstantRevealWithoutAnimation();
                }
            }
        }

        PlayResultVideo();

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.gameObject.SetActive(true);
            LobbyHUDTopBar.Instance.RefreshCurrencyUI();
        }

        SetMainRollButtonsInteractable(true);
        ToggleUIPanels(false);
        SetResultPanelActive(true);

        if (RuneInventoryUI.Instance != null && RuneInventoryUI.Instance.gameObject.activeInHierarchy)
        {
            RuneInventoryUI.Instance.RefreshInventory();
        }

        RefreshCostUI();
    }

    private void StopGachaVideoOnly()
    {
        if (videoPlayRoutine != null)
        {
            StopCoroutine(videoPlayRoutine);
            videoPlayRoutine = null;
        }

        if (gachaVideoPlayer != null)
        {
            gachaVideoPlayer.Stop();
        }

        if (videoPanelRoot != null)
        {
            if (videoCanvasGroup != null)
            {
                videoCanvasGroup.DOFade(0f, 0.2f).OnComplete(() => {
                    videoPanelRoot.SetActive(false);
                });
            }
            else
            {
                videoPanelRoot.SetActive(false);
            }
        }
    }

    public void StopAllSummoningCoroutines()
    {
        StopGachaVideoOnly();

        if (cardSpawnRoutine != null)
        {
            StopCoroutine(cardSpawnRoutine);
            cardSpawnRoutine = null;
        }
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
                    Destroy(cardParent.GetChild(i).gameObject);
                }
            }
        }
    }

    public void SetMainRollButtonsInteractable(bool interactable)
    {
        if (roll1Button != null) roll1Button.interactable = interactable;
        if (roll10Button != null) roll10Button.interactable = interactable;
    }

    public void ToggleUIPanels(bool isRolling)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(!isRolling);
            if (!isRolling) PlayResultVideo();
            else StopResultVideo();
        }
        if (closeButton != null) closeButton.SetActive(!isRolling);
        if (rerollButton != null) rerollButton.SetActive(!isRolling);
        if (skipButton != null) skipButton.SetActive(isRolling);
        if (infoButton != null) infoButton.gameObject.SetActive(!isRolling);
    }

    public void SetSkipButtonActive(bool active)
    {
        if (skipButton != null) skipButton.SetActive(active);
    }

    public void SetResultPanelActive(bool active)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(active);
            if (active) PlayResultVideo();
            else StopResultVideo();
        }
    }

    private void OnDestroy()
    {
        if (gachaVideoPlayer != null)
        {
            gachaVideoPlayer.loopPointReached -= OnVideoEndReached;
        }

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.gameObject.SetActive(true);
        }
    }
}