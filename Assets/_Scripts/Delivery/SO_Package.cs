using UnityEngine;
using System;

namespace CarePackage.Delivery
{
    [CreateAssetMenu(fileName = "Package", menuName = "CarePackage/Deliverable/Package")]
    public class SO_Package : ScriptableObject
    {
        public int Id;
        public FPackageData PackageData;
        public SO_Item Item;
        
        public UnityEngine.UI.Image AddressImage;
    }
    
    [Serializable]
    public struct FPackageData
    {
        public string Title;
        [TextArea] public string Description;
        public int MinPay;
        public int MaxPay;
        public EPackageState State;
        
        public FPackageData(string inTitle = "", string inDescription = "") : this(inTitle, inDescription, 0, 1, EPackageState.Pristine) {}

        public FPackageData(string inTitle, string inDescription, int inMinPay, int inMaxPay, EPackageState inState = EPackageState.Pristine)
        {
            Title = inTitle;
            Description = inDescription;
            MinPay = inMinPay;
            MaxPay = inMaxPay;
            State = inState;
        }
    }
    
    [Serializable]
    public class Package : IEquatable<Package>
    {
        public int Id;
        public FPackageData PackageData;
        public string ItemGUID;
        
        public bool Equals(Package other)
        {
            if (other == null) return false;
            return Id == other.Id && ItemGUID == other.ItemGUID;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Package);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Id.GetHashCode();
                hash = hash * 23 + (ItemGUID != null ? ItemGUID.GetHashCode() : 0);
                return hash;
            }
        }
        
        public static bool operator ==(Package a, Package b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(Package a, Package b)
        {
            return !(a == b);
        }
    }
}