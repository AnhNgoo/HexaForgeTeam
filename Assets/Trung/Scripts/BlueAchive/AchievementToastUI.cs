using System.Collections;
using TMPro;
using UnityEngine;

public class AchievementToastUI :
    MonoBehaviour
{
    [SerializeField]
    private GameObject VisualRoot;

    [SerializeField]
    private TMP_Text TitleText;

    [SerializeField]
    private TMP_Text DescriptionText;

    [SerializeField]
    private float showDuration = 3f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (VisualRoot != null)
        {
            VisualRoot.SetActive(false);
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
        if (TitleText != null)
        {
            TitleText.text =
                title;
        }

        if (DescriptionText != null)
        {
            DescriptionText.text =
                description;
        }

        if (VisualRoot != null)
        {
            VisualRoot.SetActive(true);
        }

        yield return new WaitForSeconds(
            showDuration);

        if (VisualRoot != null)
        {
            VisualRoot.SetActive(false);
        }

        currentRoutine = null;
    }
}