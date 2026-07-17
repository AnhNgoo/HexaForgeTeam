using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatePanel : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Bars")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider mpSlider;
    [SerializeField] private Slider staminaSlider;

    [Header("Texts")]
    [SerializeField] private TMP_Text txtLevel;
    [SerializeField] private TMP_Text txtHP;
    [SerializeField] private TMP_Text txtMP;
    [SerializeField] private TMP_Text txtStamina;
    [SerializeField] private TMP_Text txtGold;

    public void Open()
    {
        gameObject.SetActive(true);

        if (playerStats == null)
            playerStats = FindObjectOfType<PlayerStats>();

        Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (gameObject.activeSelf)
            Refresh();
    }

    private void Refresh()
    {
        if (playerStats == null)
            return;

        SetSlider(hpSlider, playerStats.currentHP, playerStats.maxHP);
        SetSlider(mpSlider, playerStats.currentMP, playerStats.maxMP);
        SetSlider(staminaSlider, playerStats.currentStamina, playerStats.maxStamina);

        if (txtLevel != null)
            txtLevel.text = "Lv. " + playerStats.level;

        if (txtHP != null)
            txtHP.text = playerStats.currentHP + " / " + playerStats.maxHP;

        if (txtMP != null)
            txtMP.text = playerStats.currentMP + " / " + playerStats.maxMP;

        if (txtStamina != null)
            txtStamina.text = playerStats.currentStamina + " / " + playerStats.maxStamina;

        if (txtGold != null && GoldManager.Instance != null)
            txtGold.text = GoldManager.Instance.CurrentGold.ToString();
    }

    private void SetSlider(Slider slider, float value, float max)
    {
        if (slider == null)
            return;

        slider.maxValue = max;
        slider.value = value;
    }
}