using UnityEngine;

namespace CarePackage.Delivery
{
    [CreateAssetMenu(fileName = "Package", menuName = "CarePackage/Deliverable/Package")]
    public class SO_Package : ScriptableObject
    {
        public int Id;
        public FPackageData PackageData;
        public SO_Item Item;
    }
    
    [System.Serializable]
    public struct FPackageData
    {
        public FPackageData(string inTitle = "", string inDescription = "", int inPay = 0, EPackageState inState = EPackageState.Pristine)
        {
            Title = inTitle;
            Description = inDescription;
            Pay = inPay;
            State = inState;
        }

        public FPackageData(int inPay, string inTitle = "", string inDescription = "", EPackageState inState = EPackageState.Pristine)
        {
            Title = inTitle;
            Description = inDescription;
            Pay = inPay;
            State = inState;
        }
        
        public string Title;
        [TextArea]
        public string Description;
        public int Pay;
        public EPackageState State;
    }
    
    [System.Serializable]
    public class Package
    {
        public int Id;
        public FPackageData PackageData;
        public string ItemGUID;
    }
}