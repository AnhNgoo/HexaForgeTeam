using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeUI : MonoBehaviour
{
    [HideInInspector] public TMP_Text titleText;
    [HideInInspector] public TMP_Text subTitleText;

    [HideInInspector] public Button claimButton;
    [HideInInspector] public GameObject darkOverlay;
    [HideInInspector] public GameObject claimedIcon;

    private void Awake()
    {
        titleText = transform.Find("Title")?.GetComponent<TMP_Text>();
        subTitleText = transform.Find("Sub-Title")?.GetComponent<TMP_Text>();

        claimButton = GetComponentInChildren<Button>(true);

        darkOverlay = transform.Find("Dark-Overlay")?.gameObject;

        Transform reward = transform.Find("Reward");

        if (reward != null)
        {
            claimedIcon = reward.GetChild(reward.childCount - 1).gameObject;
        }
    }
}