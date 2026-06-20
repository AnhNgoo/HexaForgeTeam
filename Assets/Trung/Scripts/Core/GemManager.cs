using UnityEngine;
using TMPro;

public class GemManager : MonoBehaviour
{
    public static GemManager Instance;

    [Header("Gem")]
    [SerializeField] private int currentGem = 3000;

    [Header("UI")]
    [SerializeField] private TMP_Text gemText;



   private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
    }

    currentGem =
        SaveLoadManager.Instance
        .SaveData.gem;

    UpdateGemUI();
}

    #region Add

    public void AddGem(
        int amount)
    {
        currentGem += amount;

SaveLoadManager.Instance
    .SaveData.gem =
    currentGem;

SaveLoadManager.Instance
    .SaveGame();

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

SaveLoadManager.Instance
    .SaveData.gem =
    currentGem;

SaveLoadManager.Instance
    .SaveGame();

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