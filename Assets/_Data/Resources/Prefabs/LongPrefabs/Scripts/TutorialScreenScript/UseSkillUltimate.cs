using TMPro;
using UnityEngine.InputSystem;

public class UseSkillUltimate : LoadComponents
{
    [UnityEngine.SerializeField]
    private TMP_Text keyTextUseSkillUltimate;

    protected override void LoadComponent()
    {
        if (keyTextUseSkillUltimate == null)
        {
            keyTextUseSkillUltimate = GetComponent<TMP_Text>();
        }

        if (keyTextUseSkillUltimate == null)
        {
            keyTextUseSkillUltimate = GetComponentInChildren<TMP_Text>(true);
        }
    }

    protected override void LoadComponentRuntime()
    {
        LoadComponent();
        RefreshKeyText();
    }

    private void OnEnable()
    {
        RefreshKeyText();
    }

    private void RefreshKeyText()
    {
        if (keyTextUseSkillUltimate == null)
            return;

        keyTextUseSkillUltimate.text = GetUseSkillBindingDisplay();
    }

    private string GetUseSkillBindingDisplay()
    {
        InputAction useSkillAction =
            InputManager.InputActions.Keyboard.Skill_2;

        for (int index = 0;
             index < useSkillAction.bindings.Count;
             index++)
        {
            if (useSkillAction.bindings[index].name == "E")
            {
                string displayText =
                    useSkillAction.GetBindingDisplayString(index);

                return string.IsNullOrWhiteSpace(displayText)
                    ? "E"
                    : displayText;
            }
        }

        return "E";
    }
}