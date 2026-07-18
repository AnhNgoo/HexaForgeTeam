using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class DisplayItem : LoadComponents
{
    [SerializeField] private ItemDataBase itemData;
    [SerializeField] private Image itemImage;
    [SerializeField] private DOTweenAnimation dotweenAnimation;
    protected override void LoadComponent()
    {
        if (itemImage == null)
            itemImage = transform.Find("Icon")?.GetComponent<Image>();
        if (dotweenAnimation == null)
            dotweenAnimation = GetComponent<DOTweenAnimation>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public async void SetDisplayItem(ItemDataBase itemData)
    {
        dotweenAnimation?.DORestart();
        await UniTask.Delay(500);
        if (itemData == null)
        {
            this.itemData = null;
            itemImage.gameObject.SetActive(false);
            itemImage.sprite = null;
            return;
        }

        this.itemData = itemData;
        itemImage.gameObject.SetActive(true);
        itemImage.sprite = itemData.itemIcon;
    }
}
