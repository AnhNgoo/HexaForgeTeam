using TMPro;
using UnityEngine;

public class LostGoldObject : MonoBehaviour
{
    [Header("Gold")]
    [SerializeField] private int storedGold;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI txtGold;

    public void Setup(int goldAmount)
    {
        storedGold = goldAmount;

        UpdateUI();
    }

    public void CollectGold()
    {
        if (storedGold <= 0)
            return;

        GoldManager.Instance?.AddGold(storedGold);

        Debug.Log($"Đã nhặt lại {storedGold} vàng");

        Destroy(gameObject);
    }

    private void UpdateUI()
    {
        if (txtGold == null)
            return;

        txtGold.text = $"{storedGold}";
    }
}