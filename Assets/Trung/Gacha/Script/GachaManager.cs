using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    [Header("Gem")]


    [Header("Cost")]
[Header("Rarity Rate")]

[SerializeField]
[Range(0, 100)]
private int commonRate = 60;

[SerializeField]
[Range(0, 100)]
private int rareRate = 30;

[SerializeField]
[Range(0, 100)]
private int epicRate = 9;

[SerializeField]
[Range(0, 100)]
private int legendaryRate = 1;
    [SerializeField] private int costRoll1 = 300;

    [SerializeField] private int costRoll5 = 1400;

    [Header("UI")]
    [SerializeField] private TMP_Text gemText;

    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private int lastRollCost;

[SerializeField] private int lastRollAmount;

    [Header("Card")]
    [SerializeField] private RuneCardUI cardPrefab;

    [SerializeField] private Transform cardParent;

    private readonly List<GameObject> currentCards =
        new List<GameObject>();
        [Header("Reveal")]
        [SerializeField]
private float legendaryZoomScale = 1.45f;

[SerializeField]
private float legendaryShakeDuration = 0.45f;

[SerializeField]
private float legendaryShakeStrength = 18f;

[SerializeField] private GameObject closeButton;

[SerializeField] private GameObject rerollButton;

private int revealedCardCount;
private int totalCardCount;
public static GachaManager Instance;

    private void Start()
    {

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }
    private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
    }
}

    #region Roll

    public void Roll1()
    {
        Roll(costRoll1, 1);
    }

    public void Roll5()
    {
        Roll(costRoll5, 5);
    }

    private void Roll(
        int cost,
        int amount)
    {
        if (!GemManager.Instance
    .SpendGem(cost))
{
    return;
}
lastRollCost = cost;

lastRollAmount = amount;

        ClearCards();
        revealedCardCount = 0;

totalCardCount = amount;

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
        if (closeButton != null)
{
    closeButton.SetActive(false);
}

if (rerollButton != null)
{
    rerollButton.SetActive(false);
}

        List<RuneCardUI> spawnedCards =
    new List<RuneCardUI>();

RuneCardUI legendaryCard =
    null;

for (int i = 0;
    i < amount;
    i++)
{
    RuneData rune =
        GenerateRandomRune();

    RuneInventory.Instance
        .AddRune(rune);

    RuneCardUI card =
        SpawnCard(rune);

    spawnedCards.Add(card);

    if (rune.runeRarity ==
        RuneRarity.Legendary)
    {
        legendaryCard = card;
    }
}

if (legendaryCard != null)
{
    StartCoroutine(
        PlayLegendaryReveal(
            legendaryCard,
            spawnedCards));
}

    }

    #endregion

    #region Spawn Card
    private System.Collections.IEnumerator
    PlayLegendaryReveal(
        RuneCardUI legendaryCard,
        List<RuneCardUI> allCards)
{
    yield return new WaitForSeconds(0.25f);

    RectTransform legendRect =
        legendaryCard.transform
        as RectTransform;

    Vector3 originalScale =
        legendRect.localScale;

    Vector3 originalPos =
        legendRect.localPosition;

    CanvasGroup legendGroup =
        legendaryCard
        .GetComponent<CanvasGroup>();

    if (legendGroup == null)
    {
        legendGroup =
            legendaryCard
            .gameObject
            .AddComponent<CanvasGroup>();
    }

    List<CanvasGroup> hiddenGroups =
        new List<CanvasGroup>();

    for (int i = 0;
        i < allCards.Count;
        i++)
    {
        if (allCards[i] ==
            legendaryCard)
        {
            continue;
        }

        CanvasGroup group =
            allCards[i]
            .GetComponent<CanvasGroup>();

        if (group == null)
        {
            group =
                allCards[i]
                .gameObject
                .AddComponent<CanvasGroup>();
        }

        hiddenGroups.Add(group);

        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    float timer = 0f;

    while (timer <
        legendaryShakeDuration)
    {
        timer += Time.deltaTime;

        float shakeX =
            Random.Range(
                -legendaryShakeStrength,
                legendaryShakeStrength);

        float shakeY =
            Random.Range(
                -legendaryShakeStrength,
                legendaryShakeStrength);

        legendRect.localPosition =
            originalPos +
            new Vector3(
                shakeX,
                shakeY,
                0f);

        yield return null;
    }

    legendRect.localPosition =
        originalPos;

    timer = 0f;

    Vector3 targetScale =
        originalScale *
        legendaryZoomScale;

    while (timer < 0.35f)
    {
        timer += Time.deltaTime;

        float t =
            timer / 0.35f;

        legendRect.localScale =
            Vector3.Lerp(
                originalScale,
                targetScale,
                t);

        legendRect.localPosition =
            Vector3.Lerp(
                originalPos,
                Vector3.zero,
                t);

        yield return null;
    }

    yield return new WaitForSeconds(0.25f);

    legendaryCard.ForceReveal();

    while (!legendaryCard.IsRevealed())
    {
        yield return null;
    }

    yield return new WaitForSeconds(0.35f);

    timer = 0f;

    while (timer < 0.25f)
    {
        timer += Time.deltaTime;

        float t =
            timer / 0.25f;

        legendRect.localScale =
            Vector3.Lerp(
                targetScale,
                originalScale,
                t);

        legendRect.localPosition =
            Vector3.Lerp(
                Vector3.zero,
                originalPos,
                t);

        yield return null;
    }

    legendRect.localScale =
        originalScale;

    legendRect.localPosition =
        originalPos;

    for (int i = 0;
        i < hiddenGroups.Count;
        i++)
    {
        hiddenGroups[i].alpha = 1f;
        hiddenGroups[i].blocksRaycasts = true;
        hiddenGroups[i].interactable = true;
    }
}

    private RuneCardUI SpawnCard(
    RuneData runeData)
    {
        RuneCardUI card =
            Instantiate(
                cardPrefab,
                cardParent);

        card.Setup(runeData);

        currentCards.Add(
            card.gameObject);
            return card;
    }

    private void ClearCards()
    {
        for (int i = 0;
            i < currentCards.Count;
            i++)
        {
            if (currentCards[i] != null)
            {
                Destroy(
                    currentCards[i]);
            }
        }

        currentCards.Clear();
    }

    #endregion

    #region Generate Rune

    private RuneData GenerateRandomRune()
    {
        RuneColor runeColor =
            RandomRuneColor();

        RuneRarity runeRarity =
            RandomRuneRarity();

        RuneData rune =
            new RuneData(
                runeColor,
                runeRarity);

        AssignRuneLore(rune);

GenerateAffixes(rune);

return rune;
    }

    private RuneColor RandomRuneColor()
    {
        int random =
            Random.Range(0, 100);

        if (random < 30)
        {
            return RuneColor.Red;
        }

        if (random < 65)
        {
            return RuneColor.Green;
        }

        return RuneColor.Blue;
    }

private RuneRarity RandomRuneRarity()
{
    int totalRate =
        commonRate +
        rareRate +
        epicRate +
        legendaryRate;

    if (totalRate <= 0)
    {
        return RuneRarity.Common;
    }

    int random =
        Random.Range(
            0,
            totalRate);

    if (random < commonRate)
    {
        return RuneRarity.Common;
    }

    random -= commonRate;

    if (random < rareRate)
    {
        return RuneRarity.Rare;
    }

    random -= rareRate;

    if (random < epicRate)
    {
        return RuneRarity.Epic;
    }

    return RuneRarity.Legendary;
}

    #endregion

    #region Affix

    private void GenerateAffixes(
    RuneData rune)
{
    int affixCount =
        GetAffixCount(
            rune.runeRarity);

    List<RuneStatType> usedStats =
        new List<RuneStatType>();

    for (int i = 0;
        i < affixCount;
        i++)
    {
        RuneStatType statType =
            GetRandomStat(
                usedStats);

        usedStats.Add(statType);

        float value =
            GetRandomValue(
                statType,
                rune.runeRarity);

        rune.affixes.Add(
            new RuneAffixData(
                statType,
                value));
    }
}

    private int GetAffixCount(
    RuneRarity runeRarity)
{
    switch (runeRarity)
    {
        case RuneRarity.Common:

            return 1;

        case RuneRarity.Rare:

            return 2;

        case RuneRarity.Epic:

            return 3;

        case RuneRarity.Legendary:

            return 4;
    }

    return 1;
}

    private RuneStatType GetRandomStat(
    List<RuneStatType> usedStats)
{
    List<RuneStatType> pool =
        new List<RuneStatType>()
    {
        RuneStatType.HP,
        RuneStatType.HPPercent,

        RuneStatType.MP,
        RuneStatType.MPPercent,

        RuneStatType.Stamina,
        RuneStatType.StaminaPercent,

        RuneStatType.ATK,
        RuneStatType.ATKPercent,

        RuneStatType.MATK,
        RuneStatType.MATKPercent,

        RuneStatType.DEF,
        RuneStatType.DEFPercent,

        RuneStatType.AttackSpeed,
        RuneStatType.CritChance,
        RuneStatType.CritDamage,
        RuneStatType.ArmorPenetration,
        RuneStatType.StaminaRegen,

    };

    for (int i = pool.Count - 1;
        i >= 0;
        i--)
    {
        if (usedStats.Contains(pool[i]))
        {
            pool.RemoveAt(i);
        }
    }

    return pool[
        Random.Range(
            0,
            pool.Count)];
}

    private float GetRandomValue(
    RuneStatType statType,
    RuneRarity rarity)
{
    switch (statType)
    {
        #region Flat

        case RuneStatType.HP:

            return GetValueByRarity(
                rarity,
                80f, 180f,
                180f, 350f,
                350f, 650f,
                650f, 1200f);

        case RuneStatType.MP:

            return GetValueByRarity(
                rarity,
                25f, 60f,
                60f, 120f,
                120f, 220f,
                220f, 400f);

        case RuneStatType.Stamina:

            return GetValueByRarity(
                rarity,
                15f, 40f,
                40f, 80f,
                80f, 140f,
                140f, 250f);

        case RuneStatType.ATK:

            return GetValueByRarity(
                rarity,
                3f, 8f,
                8f, 18f,
                18f, 35f,
                35f, 60f);

        case RuneStatType.MATK:

            return GetValueByRarity(
                rarity,
                3f, 8f,
                8f, 18f,
                18f, 35f,
                35f, 60f);

        case RuneStatType.DEF:

            return GetValueByRarity(
                rarity,
                2f, 6f,
                6f, 14f,
                14f, 28f,
                28f, 50f);

        #endregion

        #region Percent

        case RuneStatType.HPPercent:

            return GetValueByRarity(
                rarity,
                2f, 4f,
                4f, 7f,
                7f, 12f,
                12f, 20f);

        case RuneStatType.MPPercent:

            return GetValueByRarity(
                rarity,
                2f, 4f,
                4f, 7f,
                7f, 12f,
                12f, 18f);

        case RuneStatType.StaminaPercent:

            return GetValueByRarity(
                rarity,
                3f, 5f,
                5f, 9f,
                9f, 15f,
                15f, 25f);

        case RuneStatType.ATKPercent:

            return GetValueByRarity(
                rarity,
                2f, 4f,
                4f, 7f,
                7f, 12f,
                12f, 18f);

        case RuneStatType.MATKPercent:

            return GetValueByRarity(
                rarity,
                2f, 4f,
                4f, 7f,
                7f, 12f,
                12f, 18f);

        case RuneStatType.DEFPercent:

            return GetValueByRarity(
                rarity,
                2f, 4f,
                4f, 7f,
                7f, 12f,
                12f, 18f);

        #endregion

        #region Combat

        case RuneStatType.AttackSpeed:

            return GetValueByRarity(
                rarity,
                2f, 5f,
                5f, 9f,
                9f, 15f,
                15f, 25f);

        case RuneStatType.CritChance:

            return GetValueByRarity(
                rarity,
                1f, 3f,
                3f, 6f,
                6f, 10f,
                10f, 18f);

        case RuneStatType.CritDamage:

            return GetValueByRarity(
                rarity,
                4f, 8f,
                8f, 15f,
                15f, 25f,
                25f, 40f);

        case RuneStatType.ArmorPenetration:

            return GetValueByRarity(
                rarity,
                2f, 5f,
                5f, 9f,
                9f, 15f,
                15f, 25f);

        case RuneStatType.StaminaRegen:

            return GetValueByRarity(
                rarity,
                3f, 6f,
                6f, 10f,
                10f, 18f,
                18f, 30f);

        #endregion
    }

    return 1f;
}


private float GetValueByRarity(
    RuneRarity rarity,

    float commonMin,
    float commonMax,

    float rareMin,
    float rareMax,

    float epicMin,
    float epicMax,

    float legendaryMin,
    float legendaryMax)
{
    switch (rarity)
    {
        case RuneRarity.Common:

            return Random.Range(
                commonMin,
                commonMax);

        case RuneRarity.Rare:

            return Random.Range(
                rareMin,
                rareMax);

        case RuneRarity.Epic:

            return Random.Range(
                epicMin,
                epicMax);

        case RuneRarity.Legendary:

            return Random.Range(
                legendaryMin,
                legendaryMax);
    }

    return 1f;
}

    #endregion
    #region Rune Lore

private void AssignRuneLore(
    RuneData rune)
{
    switch (rune.runeColor)
    {
        case RuneColor.Red:

            AssignRedLore(rune);

            break;

        case RuneColor.Green:

            AssignGreenLore(rune);

            break;

        case RuneColor.Blue:

            AssignBlueLore(rune);

            break;
    }
}

#region Red

private void AssignRedLore(
    RuneData rune)
{
    switch (rune.runeRarity)
    {
        case RuneRarity.Common:

            rune.runeName =
                "Ashfang";

            rune.runeLore =
                "Its heat faded long ago, yet the scar remains.";

            break;

        case RuneRarity.Rare:

            rune.runeName =
                "Blood Oath";

            rune.runeLore =
                "The knight survived the battle. His comrades did not.";

            break;

        case RuneRarity.Epic:

            rune.runeName =
                "Heart of Ruin";

            rune.runeLore =
                "Every beat echoed like a war drum beneath the earth.";

            break;

        case RuneRarity.Legendary:

            rune.runeName =
                "Crimson Crown";

            rune.runeLore =
                "Kings burned kingdoms to wear it for a single night.";

            break;
    }
}

#endregion

#region Green

private void AssignGreenLore(
    RuneData rune)
{
    switch (rune.runeRarity)
    {
        case RuneRarity.Common:

            rune.runeName =
                "Wiltroot";

            rune.runeLore =
                "It grew where no light should ever reach.";

            break;

        case RuneRarity.Rare:

            rune.runeName =
                "Verdant Pulse";

            rune.runeLore =
                "The forest whispered back when spoken to.";

            break;

        case RuneRarity.Epic:

            rune.runeName =
                "Hollow Bloom";

            rune.runeLore =
                "Flowers fed on the dead beneath the ruins.";

            break;

        case RuneRarity.Legendary:

            rune.runeName =
                "Worldsap Core";

            rune.runeLore =
                "Its roots once held an entire civilization together.";

            break;
    }
}

#endregion

#region Blue

private void AssignBlueLore(
    RuneData rune)
{
    switch (rune.runeRarity)
    {
        case RuneRarity.Common:

            rune.runeName =
                "Frost Vein";

            rune.runeLore =
                "Cold enough to silence fear itself.";

            break;

        case RuneRarity.Rare:

            rune.runeName =
                "Moon Shard";

            rune.runeLore =
                "Fragments of a sky long forgotten.";

            break;

        case RuneRarity.Epic:

            rune.runeName =
                "Deep Current";

            rune.runeLore =
                "Something ancient moved beneath the tide.";

            break;

        case RuneRarity.Legendary:

            rune.runeName =
                "Eye of Eternity";

            rune.runeLore =
                "It watched the end before time understood death.";

            break;
    }
}

#endregion

#endregion

    #region Gem

   public void AddTestGem()
{
    GemManager.Instance
        .AddGem(5000);
}

   

    #endregion

    #region Panel

    public void CloseResultPanel()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        ClearCards();
    }

    #endregion
    public void ReRoll()
{
    if (lastRollAmount <= 0)
    {
        return;
    }

    Roll(
        lastRollCost,
        lastRollAmount);
}
public void NotifyCardRevealed()
{
    revealedCardCount++;

    if (revealedCardCount < totalCardCount)
    {
        return;
    }

    if (closeButton != null)
    {
        closeButton.SetActive(true);
    }

    if (rerollButton != null)
    {
        rerollButton.SetActive(true);
    }
}
}