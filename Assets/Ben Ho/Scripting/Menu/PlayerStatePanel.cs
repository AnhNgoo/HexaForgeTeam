using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatePanel : MenuBase
{
    [SerializeField][FoldoutGroup("Stats")] private TextMeshProUGUI txt_Level;
    [SerializeField][FoldoutGroup("Stats")] private TextMeshProUGUI txt_NextCostLevel;
    [SerializeField][FoldoutGroup("Stats")] private TextMeshProUGUI txt_HealthStat;
    [SerializeField][FoldoutGroup("Stats")] private TextMeshProUGUI txt_SpeedStat;
    [SerializeField][FoldoutGroup("Stats")] private TextMeshProUGUI txt_DamageStat;
    [SerializeField][FoldoutGroup("Stats")] private TextMeshProUGUI txt_DefenseStat;
    [SerializeField][FoldoutGroup("Stats")] private TextMeshProUGUI txt_PoisonDamageStat;
    [SerializeField][FoldoutGroup("Stats")] private TextMeshProUGUI txt_StaminaStat;
    [SerializeField][FoldoutGroup("Stats")] private TextMeshProUGUI txt_StaminaRegenStat;
    [SerializeField][FoldoutGroup("Stats")] private TextMeshProUGUI txt_MpStat;
    [SerializeField][FoldoutGroup("Stats")] private TextMeshProUGUI txt_MpRegenStat;

    public override MenuType menuType => MenuType.PlayerStateMenu;

    protected override void LoadComponent()
    {
        if (txt_Level == null)
            txt_Level = FindDeepChild("Txt_Level")?.GetComponent<TextMeshProUGUI>();
        if (txt_NextCostLevel == null)
            txt_NextCostLevel = FindDeepChild("Txt_NextCostLevel")?.GetComponent<TextMeshProUGUI>();
        if (txt_HealthStat == null)
            txt_HealthStat = FindDeepChild("Txt_HealthStat")?.GetComponent<TextMeshProUGUI>();
        if (txt_SpeedStat == null)
            txt_SpeedStat = FindDeepChild("Txt_SpeedStat")?.GetComponent<TextMeshProUGUI>();
        if (txt_DamageStat == null)
            txt_DamageStat = FindDeepChild("Txt_DamageStat")?.GetComponent<TextMeshProUGUI>();
        if (txt_DefenseStat == null)
            txt_DefenseStat = FindDeepChild("Txt_DefenseStat")?.GetComponent<TextMeshProUGUI>();
        if (txt_PoisonDamageStat == null)
            txt_PoisonDamageStat = FindDeepChild("Txt_PoisonDamageStat")?.GetComponent<TextMeshProUGUI>();
        if (txt_StaminaStat == null)
            txt_StaminaStat = FindDeepChild("Txt_StaminaStat")?.GetComponent<TextMeshProUGUI>();
        if (txt_StaminaRegenStat == null)
            txt_StaminaRegenStat = FindDeepChild("Txt_StaminaRegenStat")?.GetComponent<TextMeshProUGUI>();
        if (txt_MpStat == null)
            txt_MpStat = FindDeepChild("Txt_MpStat")?.GetComponent<TextMeshProUGUI>();
        if (txt_MpRegenStat == null)
            txt_MpRegenStat = FindDeepChild("Txt_MpRegenStat")?.GetComponent<TextMeshProUGUI>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);
        DisplayLevel();
        DisplayNextLevelCost();
        DisplayPlayerStats();
    }

    private void DisplayLevel()
    {
        if (PlayerManager.Instance == null)
            return;

        CharacterLevel characterLevel = PlayerManager.Instance.CurrentCharacterBase?.CharacterLevel;

        if (characterLevel == null)
            return;

        if (GameManager.Instance?.MapType == MapType.Lobby) // Nếu ở lobby thì chỉ hiển thị level hiện tại, không hiển thị level tiếp theo
        {
            txt_Level.text = "Level: " + characterLevel.CurrentLevel.ToString();
        }
        else // Nếu ở trong game thì hiển thị level hiện tại và level tiếp theo
        {
            if (characterLevel.CurrentLevel < characterLevel.MaxLevel) // Nếu chưa đạt cấp tối đa thì hiển thị level tiếp theo
            {
                txt_Level.text = "Level: " + characterLevel.CurrentLevel.ToString() + " -> " + (characterLevel.CurrentLevel + 1).ToString();
            }
            else // Nếu đã đạt cấp tối đa thì chỉ hiển thị level hiện tại và thông báo đã đạt cấp tối đa
            {
                txt_Level.text = "Level: " + characterLevel.CurrentLevel.ToString() + " (Max)";
            }
        }
    }

    private void DisplayNextLevelCost()
    {
        if (PlayerManager.Instance == null)
            return;

        CharacterLevel characterLevel = PlayerManager.Instance.CurrentCharacterBase?.CharacterLevel;

        if (characterLevel == null)
            return;

        if (GameManager.Instance?.MapType == MapType.Lobby) // Nếu ở lobby thì không hiển thị chi phí lên cấp
        {
            txt_NextCostLevel.text = "";
        }
        else // Nếu ở trong game thì hiển thị chi phí lên cấp
        {
            if (characterLevel.CurrentLevel < characterLevel.MaxLevel) // Nếu chưa đạt cấp tối đa thì hiển thị chi phí lên cấp tiếp theo
            {
                int nextLevelCost = characterLevel.StatGainedLevelUp.GetLevelUpCost(characterLevel.CurrentLevel + 1);
                int currentGold = GoldManager.Instance.CurrentGold;
                if (currentGold < nextLevelCost) // Nếu chưa đủ vàng thì hiển thị chi phí lên cấp tiếp theo và số vàng hiện tại, hiển thị màu đỏ để cảnh báo người chơi
                {
                    txt_NextCostLevel.text = "<color=red>Need " + currentGold.ToString() + "/" + nextLevelCost.ToString() + " Gold</color>";
                }
                else // Nếu đã đủ vàng thì hiển thị chi phí lên cấp tiếp theo và số vàng hiện tại, hiển thị màu xanh để thông báo người chơi có thể lên cấp
                {
                    txt_NextCostLevel.text = "<color=green>Need " + currentGold.ToString() + "/" + nextLevelCost.ToString() + " Gold</color>";
                }
            }
            else // Nếu đã đạt cấp tối đa thì thông báo đã đạt cấp tối đa
            {
                txt_NextCostLevel.text = "<color=yellow>Max Level</color>";
            }
        }
    }
    private void DisplayPlayerStats()
    {
        if (PlayerManager.Instance == null)
            return;

        CharacterStats stat = PlayerManager.Instance.CurrentCharacterBase?.CharacterStat.finalStats;

        if (stat == null)
            return;

        txt_HealthStat.text = $"<color=green>{stat.maxHealth}</color>";
        txt_SpeedStat.text = $"<color=green>{stat.speed}</color>";
        txt_DamageStat.text = $"<color=green>{stat.damage}</color>";
        txt_DefenseStat.text = $"<color=green>{stat.defense}</color>";
        txt_PoisonDamageStat.text = $"<color=green>{stat.poisonDamage}</color>";
        txt_StaminaStat.text = $"<color=green>{stat.stamina}</color>";
        txt_StaminaRegenStat.text = $"<color=green>{stat.staminaRegen}</color>";
        txt_MpStat.text = $"<color=green>{stat.mp}</color>";
        txt_MpRegenStat.text = $"<color=green>{stat.mpRegen}</color>";
    }


}