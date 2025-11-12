using System;
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
    
    [Serializable]
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