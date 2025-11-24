using UnityEngine;

[CreateAssetMenu(fileName = "SO_NonStoryItems", menuName = "CarePackage/Items/NonStory Items")]
public class SO_NonStoryItems : ScriptableObject
{
    public System.Collections.Generic.List<SO_Item> items;

    public SO_Item GetRandomItem()
    {
        if (items.Count == 0) return null;
        return items[Random.Range(0, items.Count)];
    }
}
