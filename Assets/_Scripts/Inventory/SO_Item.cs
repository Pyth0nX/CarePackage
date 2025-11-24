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
            return "Items/" + name;
        }
    }
}

[System.Serializable]
public struct FItemData : System.IEquatable<FItemData>
{
    public string name;
    public Sprite icon;

    public bool Equals(FItemData other)
    {
        return name == other.name && icon == other.icon;
    }

    public override bool Equals(object obj)
    {
        return obj is FItemData other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + (name != null ? name.GetHashCode() : 0);
            hash = hash * 23 + (icon != null ? icon.GetHashCode() : 0);
            return hash;
        }
    }

    public static bool operator ==(FItemData a, FItemData b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(FItemData a, FItemData b)
    {
        return !a.Equals(b);
    }
}