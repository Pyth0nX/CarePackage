using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Item", menuName = "CarePackage/Items/Item")]
public class SO_Item : ScriptableObject
{
    [SerializeField] private FItemData itemData;
    
    public FItemData ItemData => itemData;

    public string GUID
    {
        get
        {
            #if UNITY_EDITOR
            string fullPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            string ResourcePath = "Resources/";
            int startIndex = fullPath.IndexOf(ResourcePath);
            if (startIndex >= 0)
            {
                string relativePath = fullPath.Substring(startIndex + ResourcePath.Length);
                return Path.ChangeExtension(relativePath, null);
            }
            #endif
            return name;
        }
    }
}

[System.Serializable]
public struct FItemData
{
    public string name;
    public Sprite icon;
}