using TMPro;
using UnityEngine.InputSystem;

public class UseSkill : LoadComponents
{
    [UnityEngine.SerializeField]
    private TMP_Text keyTextUseSkill;

    protected override void LoadComponent()
    {
        if (keyTextUseSkill == null)
        {
            keyTextUseSkill = GetComponent<TMP_Text>();
        }

        if (keyTextUseSkill == null)
        {
            keyTextUseSkill = GetComponentInChildren<TMP_Text>(true);
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
        if (keyTextUseSkill == null)
            return;

        keyTextUseSkill.text = GetUseSkillBindingDisplay();
    }

    private string GetUseSkillBindingDisplay()
    {
        InputAction useSkillAction =
            InputManager.InputActions.Keyboard.Skill_1;

        for (int index = 0;
             index < useSkillAction.bindings.Count;
             index++)
        {
            if (useSkillAction.bindings[index].name == "Q")
            {
                string displayText =
                    useSkillAction.GetBindingDisplayString(index);

                return string.IsNullOrWhiteSpace(displayText)
                    ? "Q"
                    : displayText;
            }
        }

        return "Q";
    }
}