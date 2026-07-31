using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatePanel : MonoBehaviour
{
    [Serializable]
    public class StatRow
    {
        [Header("UI")]
        public GameObject root;
        public TMP_Text txtValue;
        public TMP_Text txtLabel;
        public Slider slider;

        [Header("Display")]
        public string label = "";
        public float sliderMax = 2000f;

        public void Set(float value, string overrideLabel = null)
        {
            if (root != null)
                root.SetActive(true);

            if (txtValue != null)
                txtValue.text = FormatNumber(value);

            if (txtLabel != null)
                txtLabel.text = string.IsNullOrEmpty(overrideLabel) ? label : overrideLabel;

            if (slider != null)
            {
                slider.minValue = 0f;
                slider.maxValue = Mathf.Max(1f, sliderMax, value);
                slider.value = Mathf.Clamp(value, 0f, slider.maxValue);
            }
        }

        private static string FormatNumber(float value)
        {
            return Mathf.RoundToInt(value).ToString();
        }
    }

    [Serializable]
    public class EffectRow
    {
        public GameObject root;
        public TMP_Text txtName;
        public TMP_Text txtDescription;

        public string effectName = "";
        [TextArea]
        public string description = "";

        public void Refresh()
        {
            if (root != null)
                root.SetActive(!string.IsNullOrEmpty(effectName));

            if (txtName != null)
                txtName.text = effectName;

            if (txtDescription != null)
                txtDescription.text = description;
        }
    }

    [Header("Runtime Sources")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private CharacterBase characterSource;
    [SerializeField] private bool autoFindPlayer = true;

    [Header("Stats")]
    [SerializeField] private StatRow healthRow;
    [SerializeField] private StatRow manaRow;
    [SerializeField] private StatRow staminaRow;

    [Header("Armaments - Equipped Weapons")]
    [SerializeField] private WeaponStatRow[] weaponDamageRows = new WeaponStatRow[4];

    [Header("Temporary Weapon Damage Values")]
    [SerializeField] private float[] weaponDamages = new float[4];

    [Header("Special Effects")]
    [SerializeField] private EffectRow[] specialEffects = new EffectRow[4];

    [Header("Refresh")]
    [SerializeField] private bool refreshWhileOpen = true;
    [SerializeField] private float refreshInterval = 0.15f;

    private float refreshTimer;

    public void Open()
    {
        gameObject.SetActive(true);
        ResolveRuntimeSources();
        Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        ResolveRuntimeSources();
        Refresh();
    }

    private void Update()
    {
        if (!refreshWhileOpen)
            return;

        refreshTimer += Time.unscaledDeltaTime;
        if (refreshTimer < refreshInterval)
            return;

        refreshTimer = 0f;
        Refresh();
    }

    public void Refresh()
    {
        ResolveRuntimeSources();

        CharacterStats characterStats = GetCharacterStats();

        float maxHP = GetMaxHP(characterStats);
        float currentHP = GetCurrentHP(maxHP);

        float maxMP = GetMaxMP();
        float currentMP = GetCurrentMP(maxMP);

        float maxStamina = GetMaxStamina(characterStats);
        float currentStamina = GetCurrentStamina(maxStamina);

        if (healthRow != null)
            healthRow.Set(currentHP, "Health");

        if (manaRow != null)
            manaRow.Set(currentMP, "Mana");

        if (staminaRow != null)
            staminaRow.Set(currentStamina, "Stamina");

        RefreshWeaponDamages();

        RefreshSpecialEffects();
    }

    private void ResolveRuntimeSources()
    {
        if (!autoFindPlayer)
            return;

        if (playerStats == null)
            playerStats = FindObjectOfType<PlayerStats>();

        if (characterSource == null)
            characterSource = FindObjectOfType<CharacterBase>();
    }

    private CharacterStats GetCharacterStats()
    {
        if (characterSource == null)
            return null;

        if (characterSource.CharacterData == null)
            return null;

        return characterSource.CharacterData.stats;
    }

    private float GetMaxHP(CharacterStats characterStats)
    {
        if (playerStats != null && playerStats.maxHP > 0f)
            return playerStats.maxHP;

        if (characterStats != null && characterStats.maxHealth > 0f)
            return characterStats.maxHealth;

        return 1f;
    }

    private float GetCurrentHP(float maxHP)
    {
        if (playerStats != null)
            return Mathf.Clamp(playerStats.currentHP, 0f, maxHP);

        return maxHP;
    }

    private float GetMaxMP()
    {
        if (playerStats != null && playerStats.maxMP > 0f)
            return playerStats.maxMP;

        return 1f;
    }

    private float GetCurrentMP(float maxMP)
    {
        if (playerStats != null)
            return Mathf.Clamp(playerStats.currentMP, 0f, maxMP);

        return maxMP;
    }

    private float GetMaxStamina(CharacterStats characterStats)
    {
        if (playerStats != null && playerStats.maxStamina > 0f)
            return playerStats.maxStamina;

        if (characterStats != null && characterStats.stamina > 0f)
            return characterStats.stamina;

        return 1f;
    }

    private float GetCurrentStamina(float maxStamina)
    {
        if (playerStats != null)
            return Mathf.Clamp(playerStats.currentStamina, 0f, maxStamina);

        return maxStamina;
    }

    [Serializable]
    public class WeaponStatRow
    {
        public GameObject root;
        public TMP_Text txtValue;
        public TMP_Text txtLabel;
        public Slider slider;

        public string emptyText = "-";
        public float sliderMax = 2000f;

        public void SetWeapon(float damage, string label = "Damage")
        {
            if (root != null)
                root.SetActive(true);

            if (txtValue != null)
                txtValue.text = Mathf.RoundToInt(damage).ToString();

            if (txtLabel != null)
                txtLabel.text = label;

            if (slider != null)
            {
                slider.minValue = 0f;
                slider.maxValue = Mathf.Max(1f, sliderMax, damage);
                slider.value = Mathf.Clamp(damage, 0f, slider.maxValue);
            }
        }

        public void SetEmpty()
        {
            if (root != null)
                root.SetActive(true);

            if (txtValue != null)
                txtValue.text = emptyText;

            if (txtLabel != null)
                txtLabel.text = "Damage";

            if (slider != null)
                slider.value = 0f;
        }
    }

    private void RefreshSpecialEffects()
    {
        if (specialEffects == null)
            return;

        for (int i = 0; i < specialEffects.Length; i++)
        {
            if (specialEffects[i] != null)
                specialEffects[i].Refresh();
        }
    }

    private void RefreshWeaponDamages()
    {
        if (weaponDamageRows == null)
            return;

        for (int i = 0; i < weaponDamageRows.Length; i++)
        {
            if (weaponDamageRows[i] == null)
                continue;

            if (weaponDamages != null && i < weaponDamages.Length && weaponDamages[i] > 0f)
                weaponDamageRows[i].SetWeapon(weaponDamages[i]);
            else
                weaponDamageRows[i].SetEmpty();
        }
    }
}