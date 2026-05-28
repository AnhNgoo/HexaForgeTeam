using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    [Header("Gem")]
    [SerializeField] private int currentGem = 3000;

    [Header("Cost")]
    [SerializeField] private int costRoll1 = 300;

    [SerializeField] private int costRoll5 = 1400;

    [Header("UI")]
    [SerializeField] private TMP_Text gemText;

    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;

    [Header("Card")]
    [SerializeField] private RuneCardUI cardPrefab;

    [SerializeField] private Transform cardParent;

    private readonly List<GameObject> currentCards =
        new List<GameObject>();

    private void Start()
    {
        UpdateGemUI();

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
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
        if (currentGem < cost)
        {
            return;
        }

        currentGem -= cost;

        ClearCards();

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        for (int i = 0;
            i < amount;
            i++)
        {
            RuneData rune =
                GenerateRandomRune();

            RuneInventory.Instance
                .AddRune(rune);

            SpawnCard(rune);
        }

        UpdateGemUI();
    }

    #endregion

    #region Spawn Card

    private void SpawnCard(
        RuneData runeData)
    {
        RuneCardUI card =
            Instantiate(
                cardPrefab,
                cardParent);

        card.Setup(runeData);

        currentCards.Add(
            card.gameObject);
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
        int random =
            Random.Range(0, 100);

        if (random < 60)
        {
            return RuneRarity.Common;
        }

        if (random < 90)
        {
            return RuneRarity.Rare;
        }

        if (random < 99)
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
                GetRandomValue(statType);

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

                return Random.Range(2, 4);

            case RuneRarity.Legendary:

                return 3;
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
            RuneStatType.MP,
            RuneStatType.Stamina,

            RuneStatType.ATK,
            RuneStatType.MATK,
            RuneStatType.DEF,

            RuneStatType.AttackSpeed,
            RuneStatType.CooldownReduction,
            RuneStatType.CritChance,
            RuneStatType.CritDamage,
            RuneStatType.ArmorPenetration,
            RuneStatType.MoveSpeed,
            RuneStatType.StaminaRegen
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
        RuneStatType statType)
    {
        switch (statType)
        {
            case RuneStatType.HP:
                return Random.Range(100f, 800f);

            case RuneStatType.MP:
                return Random.Range(40f, 200f);

            case RuneStatType.Stamina:
                return Random.Range(20f, 120f);

            case RuneStatType.ATK:
                return Random.Range(5f, 30f);

            case RuneStatType.MATK:
                return Random.Range(5f, 30f);

            case RuneStatType.DEF:
                return Random.Range(3f, 20f);

            case RuneStatType.AttackSpeed:
                return Random.Range(3f, 15f);

            case RuneStatType.CooldownReduction:
                return Random.Range(3f, 12f);

            case RuneStatType.CritChance:
                return Random.Range(2f, 10f);

            case RuneStatType.CritDamage:
                return Random.Range(5f, 20f);

            case RuneStatType.ArmorPenetration:
                return Random.Range(5f, 18f);

            case RuneStatType.MoveSpeed:
                return Random.Range(3f, 12f);

            case RuneStatType.StaminaRegen:
                return Random.Range(4f, 15f);
        }

        return 1f;
    }

    #endregion

    #region Gem

    public void AddTestGem()
    {
        currentGem += 5000;

        UpdateGemUI();
    }

    private void UpdateGemUI()
    {
        if (gemText != null)
        {
            gemText.text =
                $"Gem: {currentGem}";
        }
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
}