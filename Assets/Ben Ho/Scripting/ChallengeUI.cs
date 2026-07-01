using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeUI : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text subTitleText;

    public Button claimButton;

    public GameObject darkOverlay;
    public GameObject claimedIcon;

    private void Awake()
    {
        titleText = transform.Find("Title")?.GetComponent<TMP_Text>();

        subTitleText = transform.Find("Sub-Title")?.GetComponent<TMP_Text>();

        claimButton = GetComponentInChildren<Button>(true);

        darkOverlay = transform.Find("Dark-Overlay")?.gameObject;

        Transform reward = transform.Find("Reward");

        if (reward != null && reward.childCount > 0)
        {
            claimedIcon =
                reward.GetChild(reward.childCount - 1).gameObject;
        }
    }
}