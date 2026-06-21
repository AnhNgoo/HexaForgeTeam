using UnityEngine;

public class PlayerLostGoldSpawner : MonoBehaviour
{
    [Header("Lost Gold")]
    [SerializeField] private LostGoldObject lostGoldPrefab;

    private GameObject currentLostGoldObject;

    public void DropGold()
    {
        if (GoldManager.Instance == null)
            return;

        int currentGold = GoldManager.Instance.CurrentGold;

        if (currentGold <= 0)
            return;

        if (currentLostGoldObject != null)
        {
            Destroy(currentLostGoldObject);
        }

        currentLostGoldObject = Instantiate(
            lostGoldPrefab.gameObject,
            transform.position,
            Quaternion.identity
        );

        LostGoldObject lostGold =
            currentLostGoldObject.GetComponent<LostGoldObject>();

        lostGold.Setup(currentGold);

        GoldManager.Instance.ResetGold();

        Debug.Log($"Đã làm rơi {currentGold} vàng");
    }
}