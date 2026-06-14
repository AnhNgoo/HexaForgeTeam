using System.Collections;
using TMPro;
using UnityEngine;

public class AchievementToastUI :
    MonoBehaviour
{
    [SerializeField]
    private GameObject visualRoot;

    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private TMP_Text descriptionText;

    [SerializeField]
    private float showDuration = 3f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (visualRoot != null)
        {
            visualRoot.SetActive(false);
        }
    }

    public void ShowToast(
        string title,
        string description)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(
                currentRoutine);
        }

        currentRoutine =
            StartCoroutine(
                ShowRoutine(
                    title,
                    description));
    }

    private IEnumerator ShowRoutine(
        string title,
        string description)
    {
        if (titleText != null)
        {
            titleText.text =
                title;
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                description;
        }

        if (visualRoot != null)
        {
            visualRoot.SetActive(true);
        }

        yield return new WaitForSeconds(
            showDuration);

        if (visualRoot != null)
        {
            visualRoot.SetActive(false);
        }

        currentRoutine = null;
    }
}