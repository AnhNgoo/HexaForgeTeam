using UnityEngine;

[CreateAssetMenu(fileName = "New Gem", menuName = "Inventory/Gem Data")]
public class GemData : ScriptableObject
{
    public string gemName;
    public Sprite gemIcon;
    public int rarity;
}