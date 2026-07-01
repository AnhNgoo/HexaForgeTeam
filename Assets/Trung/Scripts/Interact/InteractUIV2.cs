using System.Collections.Generic;
using UnityEngine;

public class InteractUIV2 : MonoBehaviour
{
    public static InteractUIV2 Instance;

    [Header("Root")]
    [SerializeField]
    private GameObject root;

    [Header("Content")]
    [SerializeField]
    private Transform content;

    [SerializeField]
    private InteractItemUIV2 itemPrefab;

    private readonly List<InteractItemUIV2> itemPool =
        new List<InteractItemUIV2>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Hide();
    }

    #region Public

    public void Refresh(
        List<InteractV2> interactList,
        int selectedIndex)
    {
        if (interactList == null ||
            interactList.Count == 0)
        {
            Hide();

            return;
        }

        Show();

        EnsurePool(
            interactList.Count);

        for (int i = 0;
            i < itemPool.Count;
            i++)
        {
            if (i >= interactList.Count)
            {
                itemPool[i]
                    .gameObject
                    .SetActive(false);

                continue;
            }

            itemPool[i]
                .gameObject
                .SetActive(true);

            itemPool[i]
                .Setup(
                    interactList[i],
                    i == selectedIndex);
        }
    }

    #endregion

    #region Pool

    private void EnsurePool(
        int count)
    {
        while (itemPool.Count < count)
        {
            InteractItemUIV2 item =
                Instantiate(
                    itemPrefab,
                    content);

            item.gameObject
                .SetActive(false);

            itemPool.Add(item);
        }
    }

    #endregion

    #region Show Hide

    public void Show()
    {
        if (root == null)
        {
            return;
        }

        root.SetActive(true);
    }

    public void Hide()
    {
        if (root == null)
        {
            return;
        }

        root.SetActive(false);
    }

    #endregion
}