using TMPro;
using UnityEngine;

public class AutoLocalizedText : MonoBehaviour
{
    [SerializeField]
    private string key;

    private TMP_Text txt;

    private void Awake()
    {
        txt = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        txt.text =
            LocalizationManager.Instance.Get(key);
    }
}