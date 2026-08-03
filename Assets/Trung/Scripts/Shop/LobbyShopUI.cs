using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyShopUI : LoadComponents
{
    public static LobbyShopUI Instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject shopPanelRoot;

    [Header("Grid List Config")]
    [SerializeField] private Transform gridContentParent;
    [SerializeField] private ShopItemCardUI shopCardPrefab;

    [Header("Shop Database (Kéo thả các file ScriptableObject vào đây)")]
    [SerializeField] private List<ShopItemSO> shopItemsDatabase = new List<ShopItemSO>();

    [Header("Buttons")]
    [SerializeField] private Button closeButton;

    private List<ShopItemCardUI> spawnedCards = new List<ShopItemCardUI>();

    protected override void Awake()
    {
        base.Awake();

        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseShop);
        }

        InitializeShopGrid();
    }

    private void OnEnable()
    {
        RefreshShopUI();
    }

    public void OpenShop()
    {
        if (shopPanelRoot != null)
        {
            shopPanelRoot.SetActive(true);
        }

        RefreshShopUI();
    }

    public void CloseShop()
    {
        HideShopPanel();

        // Chuyển menu về trạng thái mặc định của Sảnh để MouseManager tự động ẩn chuột
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(MenuType.DefaultLobbyInputMenu);
        }

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = false;
            InteractManagerV2.Instance.ForceRefresh();
        }
    }

    public void HideShopPanel()
    {
        // Đóng sạch cả 2 Popup nếu còn lỡ bật khi tắt Shop
        if (ShopRuneSelectionPopupUI.Instance != null && ShopRuneSelectionPopupUI.Instance.IsPopupActive())
        {
            ShopRuneSelectionPopupUI.Instance.HidePopup();
        }

        if (ShopQuantityPopupUI.Instance != null && ShopQuantityPopupUI.Instance.IsPopupActive())
        {
            ShopQuantityPopupUI.Instance.HidePopup();
        }

        if (shopPanelRoot != null)
        {
            shopPanelRoot.SetActive(false);
        }
    }

    /// <summary>
    /// Xử lý phím ESC theo tầng ưu tiên: Popup Chọn Ngọc -> Popup Số Lượng -> Đóng Shop
    /// </summary>
    public bool HandleEscapeKey()
    {
        // Ưu tiên 1: Đóng Popup chọn Ngọc tùy chỉnh
        if (ShopRuneSelectionPopupUI.Instance != null && ShopRuneSelectionPopupUI.Instance.IsPopupActive())
        {
            ShopRuneSelectionPopupUI.Instance.HidePopup();
            return true;
        }

        // Ưu tiên 2: Đóng Popup chọn số lượng mua
        if (ShopQuantityPopupUI.Instance != null && ShopQuantityPopupUI.Instance.IsPopupActive())
        {
            ShopQuantityPopupUI.Instance.HidePopup();
            return true;
        }

        return false;
    }

    public bool IsShopPanelActive()
    {
        return shopPanelRoot != null && shopPanelRoot.activeInHierarchy;
    }

    public void InitializeShopGrid()
    {
        ClearGrid();

        if (gridContentParent == null || shopCardPrefab == null || shopItemsDatabase == null) return;

        for (int i = 0; i < shopItemsDatabase.Count; i++)
        {
            if (shopItemsDatabase[i] == null) continue;

            ShopItemCardUI card = Instantiate(shopCardPrefab, gridContentParent);
            card.SetupCard(shopItemsDatabase[i]);
            spawnedCards.Add(card);
        }
    }

    public void RefreshShopUI()
    {
        for (int i = 0; i < spawnedCards.Count; i++)
        {
            if (spawnedCards[i] != null)
            {
                spawnedCards[i].RefreshCardUI();
            }
        }
    }

    private void ClearGrid()
    {
        spawnedCards.Clear();
        if (gridContentParent != null)
        {
            for (int i = gridContentParent.childCount - 1; i >= 0; i--)
            {
                Destroy(gridContentParent.GetChild(i).gameObject);
            }
        }
    }

    protected override void LoadComponent()
    {
        if (shopPanelRoot == null)
        {
            shopPanelRoot = transform.Find("ShopPanel")?.gameObject;
            if (shopPanelRoot == null) shopPanelRoot = gameObject;
        }

        if (gridContentParent == null)
        {
            gridContentParent = transform.Find("GridContent") ?? transform.Find("ShopPanel/GridContent") ?? transform.Find("ShopPanel/Scroll View/Viewport/Content");
        }

        if (closeButton == null)
        {
            closeButton = transform.Find("CloseButton")?.GetComponent<Button>() ?? transform.Find("ShopPanel/CloseButton")?.GetComponent<Button>();
        }
    }

    protected override void LoadComponentRuntime()
    {
    }
}