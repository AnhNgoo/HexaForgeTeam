using TMPro;
using UnityEngine;

public class UIGoldText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtGold;

    private void Awake()
    {
        if (txtGold == null)
            txtGold = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (GoldManager.Instance == null)
            return;

        txtGold.text = $"Gold : {GoldManager.Instance.CurrentGold}";
    }
}