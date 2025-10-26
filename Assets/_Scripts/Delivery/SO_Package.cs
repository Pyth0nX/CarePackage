using UnityEngine;
using TMPro;

namespace CarePackage.Delivery
{
    [CreateAssetMenu(fileName = "Package", menuName = "CarePackage/Deliverable/Package")]
    public class SO_Package : ScriptableObject
    {
        public int Id;
        public int Pay;
        public FPackageData PackageData;
        public SO_Item Item;
    }
    
    [System.Serializable]
    public struct FPackageData
    {
        public string Title;
        [TextArea]
        public string Description;
    }
    
    [System.Serializable]
    public class Package
    {
        public int Id;
        public int Pay;
        public FPackageData PackageData;
        public string ItemGUID;
    }
}