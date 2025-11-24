using UnityEngine;

[CreateAssetMenu(fileName = "SO_PredeterminedAddresses", menuName = "CarePackage/Deliverable/PredeterminedAddresses")]
public class SO_PredeterminedAddresses : ScriptableObject
{
    [System.Serializable]
    public class PackageImageEntry
    {
        public int Id;
        public Sprite Image;
        public string Address;
    }

    public System.Collections.Generic.List<PackageImageEntry> addresses = new();

    public Sprite GetImageForId(int id)
    {
        var entry = GetEntryForId(id);
        return entry != null ? entry.Image : null;
    }

    public string GetAddressForId(int id)
    {
        if (addresses.Count == 0 && id <= addresses.Count) return "Unknown Address";
        return GetEntryForId(id).Address;
    }
    
    public PackageImageEntry GetEntryForId(int id)
    {
        return addresses.Find(a => a.Id == id);
    }
}
