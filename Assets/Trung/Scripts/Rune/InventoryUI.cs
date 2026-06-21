using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;    
using UnityEngine.UI;

public class InventoryUI :
    MonoBehaviour
{
    public static InventoryUI Instance;
    [Header("Panel")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Card")]
    [SerializeField] private RuneCardUI cardPrefab;
    [Header("Multi Delete")]
[SerializeField] private bool isDeleteMode;
[SerializeField] private GameObject deleteButton;
[SerializeField] private GameObject filterButton;

[SerializeField] private GameObject filterPanel;
[Header("Filter Highlight")]


[SerializeField]
private GameObject commonHighlight;

[SerializeField]
private GameObject rareHighlight;

[SerializeField]
private GameObject epicHighlight;

[SerializeField]
private GameObject legendaryHighlight;

[SerializeField]
private GameObject redHighlight;

[SerializeField]
private GameObject greenHighlight;

[SerializeField]
private GameObject blueHighlight;


private bool commonSelected;
private bool rareSelected;
private bool epicSelected;
private bool legendarySelected;
private bool redSelected;
private bool greenSelected;
private bool blueSelected;

[Header("Select Text")]

[SerializeField] private TMP_Text selectModeText;
[SerializeField] private TMP_Text deleteModeText;

[SerializeField] private GameObject selectAllButton;


private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
    }
}
    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        if (deleteButton != null)
{
    deleteButton.SetActive(false);
}
if (filterButton != null)
{
    filterButton.SetActive(false);
}

if (filterPanel != null)
{
    filterPanel.SetActive(false);
}
if (selectAllButton != null)
{
    selectAllButton.SetActive(false);
}
    }

    // private void OnEnable()
    // {
    //     RefreshInventory();
    // }

    public void OpenInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }

        RefreshInventory();
    }

    public void CloseInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    public void RefreshInventory()
    {
        Canvas.ForceUpdateCanvases();
        ClearInventory();

        if (RuneInventoryManager.Instance == null)
        {
            return;
        }

        List<RuneData> sortedRunes =
    new List<RuneData>(
        RuneInventoryManager.Instance.runes);

sortedRunes.Sort(
    (a, b) =>
    b.runeRarity.CompareTo(
        a.runeRarity));

for (int i = 0;
    i < sortedRunes.Count;
    i++)
{
    SpawnCard(
        sortedRunes[i]);
}
Canvas.ForceUpdateCanvases();

if (scrollRect != null)
{
    scrollRect.verticalNormalizedPosition = 1f;
}
    }

    private void SpawnCard(
        RuneData runeData)
    {
        RuneCardUI card =
            Instantiate(
                cardPrefab,
                contentParent);

        card.Setup(
            runeData,
            false);
    }

    private void ClearInventory()
    {
        for (int i =
            contentParent.childCount - 1;
            i >= 0;
            i--)
        {
            Destroy(
                contentParent
                .GetChild(i)
                .gameObject);
        }
    }
    public bool IsDeleteMode()
{
    return isDeleteMode;
}

public void ToggleDeleteMode()
{
    isDeleteMode = !isDeleteMode;

    if (deleteButton != null)
    {
        deleteButton.SetActive(
            isDeleteMode);
    }

    if (filterButton != null)
    {
        filterButton.SetActive(
            isDeleteMode);
    }

    if (selectAllButton != null)
    {
        selectAllButton.SetActive(
            isDeleteMode);
    }

    if (deleteModeText != null)
    {
        if (isDeleteMode)
        {
            deleteModeText.text =
                "Cancel Select";
        }
        else
        {
            deleteModeText.text =
                "Select Mode";
        }
    }

    if (!isDeleteMode)
    {
        CloseFilterPanel();
    }

    UpdateSelectModeText();

    RefreshInventory();
}

public void DeleteSelectedRunes()
{
    RuneCardUI[] cards =
        contentParent
        .GetComponentsInChildren<RuneCardUI>();

    for (int i = cards.Length - 1;
        i >= 0;
        i--)
    {
        if (!cards[i].IsSelected())
        {
            continue;
        }

        RuneData runeData =
            cards[i].GetRuneData();

        if (runeData == null)
        {
            continue;
        }

        int refundGem =
            GetRefundGemByRarity(
                runeData.runeRarity);

        GemManager.Instance
            .AddGem(refundGem);

        RuneInventoryManager.Instance
            .RemoveRune(
                runeData.runeID);
    }

    RefreshInventory();
}
private int GetRefundGemByRarity(
    RuneRarity rarity)
{
    switch (rarity)
    {
        case RuneRarity.Common:
            return 50;

        case RuneRarity.Rare:
            return 120;

        case RuneRarity.Epic:
            return 300;

        case RuneRarity.Legendary:
            return 800;
    }

    return 0;
}
#region Filter
private void Update()
{
    if (filterPanel == null)
    {
        return;
    }

    if (!filterPanel.activeSelf)
    {
        return;
    }

    if (!Input.GetMouseButtonDown(0))
    {
        return;
    }

    bool clickInside =
        RectTransformUtility
        .RectangleContainsScreenPoint(
            filterPanel.transform
                as RectTransform,
            Input.mousePosition,
            null);

    if (clickInside)
    {
        return;
    }

    if (EventSystem.current != null &&
        EventSystem.current
        .IsPointerOverGameObject())
    {
        CloseFilterPanel();
    }
}

public void OpenFilterPanel()
{
    if (filterPanel != null)
    {
        filterPanel.SetActive(true);
    }
}

public void CloseFilterPanel()
{
    if (filterPanel != null)
    {
        filterPanel.SetActive(false);
    }
}

private void UpdateFilterVisual()
{
    if (commonHighlight != null)
    {
        commonHighlight.SetActive(
            commonSelected);
    }

    if (rareHighlight != null)
    {
        rareHighlight.SetActive(
            rareSelected);
    }

    if (epicHighlight != null)
    {
        epicHighlight.SetActive(
            epicSelected);
    }

    if (legendaryHighlight != null)
    {
        legendaryHighlight.SetActive(
            legendarySelected);
    }
if (redHighlight != null)
{
    redHighlight.SetActive(
        redSelected);
}

if (greenHighlight != null)
{
    greenHighlight.SetActive(
        greenSelected);
}

if (blueHighlight != null)
{
    blueHighlight.SetActive(
        blueSelected);
}

}


private void ApplyFilterSelection()
{
    RuneCardUI[] cards =
        contentParent
        .GetComponentsInChildren<RuneCardUI>();

    for (int i = 0;
        i < cards.Length;
        i++)
    {
        RuneData runeData =
            cards[i].GetRuneData();

        if (runeData == null)
        {
            continue;
        }

        bool raritySelected = false;

        switch (runeData.runeRarity)
        {
            case RuneRarity.Common:

                raritySelected =
                    commonSelected;

                break;

            case RuneRarity.Rare:

                raritySelected =
                    rareSelected;

                break;

            case RuneRarity.Epic:

                raritySelected =
                    epicSelected;

                break;

            case RuneRarity.Legendary:

                raritySelected =
                    legendarySelected;

                break;
        }

        bool colorSelected = false;

        switch (runeData.runeColor)
        {
            case RuneColor.Red:

                colorSelected =
                    redSelected;

                break;

            case RuneColor.Green:

                colorSelected =
                    greenSelected;

                break;

            case RuneColor.Blue:

                colorSelected =
                    blueSelected;

                break;
        }

        bool finalSelected =
            raritySelected ||
            colorSelected;

        cards[i].SetSelected(
            finalSelected);
    }

    UpdateSelectModeText();
}

public void SelectCommon()
{
    commonSelected =
        !commonSelected;

    UpdateFilterVisual();

    ApplyFilterSelection();
}

public void SelectRare()
{
    rareSelected =
        !rareSelected;

    UpdateFilterVisual();

    ApplyFilterSelection();
}




public void SelectEpic()
{
    epicSelected =
        !epicSelected;

    UpdateFilterVisual();

    ApplyFilterSelection();
}




public void SelectLegendary()
{
    legendarySelected =
        !legendarySelected;

    UpdateFilterVisual();

    ApplyFilterSelection();
}


public void SelectRed()
{
    redSelected =
        !redSelected;

    UpdateFilterVisual();

    ApplyFilterSelection();
}

public void SelectGreen()
{
    greenSelected =
        !greenSelected;

    UpdateFilterVisual();

    ApplyFilterSelection();
}

public void SelectBlue()
{
    blueSelected =
        !blueSelected;

    UpdateFilterVisual();

    ApplyFilterSelection();
}




#endregion
public void UpdateSelectModeText()
{
    if (selectModeText == null)
    {
        return;
    }

    RuneCardUI[] cards =
        contentParent
        .GetComponentsInChildren<RuneCardUI>();

    bool hasUnselected = false;

    for (int i = 0;
        i < cards.Length;
        i++)
    {
        if (!cards[i].IsSelected())
        {
            hasUnselected = true;

            break;
        }
    }

    if (hasUnselected)
    {
        selectModeText.text =
            "Select All";
    }
    else
    {
        selectModeText.text =
            "Deselect All";
    }
}
public void ToggleSelectAll()
{
    RuneCardUI[] cards =
        contentParent
        .GetComponentsInChildren<RuneCardUI>();

    bool hasUnselected = false;

    for (int i = 0;
        i < cards.Length;
        i++)
    {
        if (!cards[i].IsSelected())
        {
            hasUnselected = true;

            break;
        }
    }

    for (int i = 0;
        i < cards.Length;
        i++)
    {
        cards[i].SetSelected(
            hasUnselected);
    }

    UpdateSelectModeText();
}
}