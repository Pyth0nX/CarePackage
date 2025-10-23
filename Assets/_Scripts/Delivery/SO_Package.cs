using UnityEngine;

namespace CarePackage.Delivery
{
    [CreateAssetMenu(fileName = "Package", menuName = "CarePackage/Deliverable/Package")]
    public class SO_Package : IDeliverable
    {
        [SerializeField] private FPackageData deliveryData;
        [SerializeField] private SO_Item item;

        public FPackageData PackageData => deliveryData;
    }
    
    [System.Serializable]
    public struct FPackageData
    {
        public string Title;
        public string Description;
    }
}