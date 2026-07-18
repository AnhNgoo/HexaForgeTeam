using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "ScriptableObjects/ItemData", order = 1)]
public class ItemDataBase : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    [TextArea(3, 10)] public string itemDescription;
}
