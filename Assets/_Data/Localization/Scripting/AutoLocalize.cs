using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;

public class AutoLocalize : MonoBehaviour
{
    [System.Serializable]
    public class TextLocalize
    {
        public TMP_Text textComponent;
        public string tableReference = "UI";
        public string key;
    }

    public TextLocalize[] textsToLocalize;
    
    private List<LocalizedString> localizedStrings = new List<LocalizedString>();
    private List<TMP_Text> textComponents = new List<TMP_Text>();

    private void Start()
    {
        // Subscribe to locale changed event
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        
        foreach (var textData in textsToLocalize)
        {
            if (textData.textComponent != null && !string.IsNullOrEmpty(textData.key))
            {
                // Create LocalizedString
                var localizedString = new LocalizedString(textData.key, textData.tableReference);
                localizedStrings.Add(localizedString);
                textComponents.Add(textData.textComponent);
                
                // Set initial value
                UpdateText(localizedString, textData.textComponent);
            }
        }
    }

    private void OnLocaleChanged(Locale locale)
    {
        // Update all texts when locale changes
        for (int i = 0; i < localizedStrings.Count; i++)
        {
            if (i < textComponents.Count)
            {
                UpdateText(localizedStrings[i], textComponents[i]);
            }
        }
    }

    private void UpdateText(LocalizedString localizedString, TMP_Text textComponent)
    {
        if (textComponent != null)
        {
            textComponent.text = localizedString.GetLocalizedString();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from event
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        
        localizedStrings.Clear();
        textComponents.Clear();
    }
}