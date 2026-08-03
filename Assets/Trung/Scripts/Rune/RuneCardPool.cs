using System.Collections.Generic;
using UnityEngine;

public class RuneCardPool : MonoBehaviour
{
    public static RuneCardPool Instance { get; private set; }

    [SerializeField] private RuneCardUI runeCardPrefab;
    [SerializeField] private int initialSize = 20;

    private Queue<RuneCardUI> pool = new Queue<RuneCardUI>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        PrewarmPool();
    }

    private void PrewarmPool()
    {
        if (runeCardPrefab == null) return;
        for (int i = 0; i < initialSize; i++)
        {
            RuneCardUI card = Instantiate(runeCardPrefab, transform);
            card.gameObject.SetActive(false);
            pool.Enqueue(card);
        }
    }

    public RuneCardUI GetCard(Transform parent)
    {
        RuneCardUI card;
        if (pool.Count > 0)
        {
            card = pool.Dequeue();
        }
        else
        {
            card = Instantiate(runeCardPrefab, transform);
        }

        card.transform.SetParent(parent, false);
        card.gameObject.SetActive(true);
        return card;
    }

    public void ReturnCard(RuneCardUI card)
    {
        if (card == null) return;
        card.gameObject.SetActive(false);
        card.transform.SetParent(transform, false);
        pool.Enqueue(card);
    }
}