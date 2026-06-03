using UnityEngine;
using TMPro;

public class GemManager : MonoBehaviour
{
    public static GemManager Instance;

    [Header("Gem")]
    [SerializeField] private int currentGem = 3000;

    [Header("UI")]
    [SerializeField] private TMP_Text gemText;

    private const string GemSaveKey =
        "PLAYER_GEM";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        LoadGem();

        UpdateGemUI();
    }

    #region Add

    public void AddGem(
        int amount)
    {
        currentGem += amount;

        SaveGem();

        UpdateGemUI();
    }

    #endregion

    #region Spend

    public bool SpendGem(
        int amount)
    {
        if (currentGem < amount)
        {
            return false;
        }

        currentGem -= amount;

        SaveGem();

        UpdateGemUI();

        return true;
    }

    #endregion

    #region Get

    public int GetCurrentGem()
    {
        return currentGem;
    }

    #endregion

    #region Save Load

    private void SaveGem()
    {
        PlayerPrefs.SetInt(
            GemSaveKey,
            currentGem);

        PlayerPrefs.Save();
    }

    private void LoadGem()
    {
        currentGem =
            PlayerPrefs.GetInt(
                GemSaveKey,
                3000);
    }

    #endregion

    #region UI

    private void UpdateGemUI()
    {
        if (gemText != null)
        {
            gemText.text =
                currentGem.ToString();
        }
    }

    #endregion
}