using UnityEngine;

[CreateAssetMenu(fileName = "SO_Item", menuName = "CarePackage/Items/Item")]
public class SO_Item : ScriptableObject
{
    [SerializeField] private FItemData itemData;
}

[System.Serializable]
public struct FItemData
{
    public string name;
    public Sprite icon;
}