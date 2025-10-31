using System.Collections.Generic;
using UnityEngine;

namespace CarePackage.Delivery
{
    public class DeliveryUitilities : MonoBehaviour
    {
        public static Package ToPackage(SO_Package soPackage)
        {
            return new Package
            {
                Id = soPackage.Id,
                PackageData = soPackage.PackageData,
                ItemGUID = soPackage.Item != null ? soPackage.Item.GUID : string.Empty
            };
        }
        
        public static SO_Package ToScriptableObject(Package package)
        {
            SO_Item item = InventoryUtilities.LoadItem(package.ItemGUID);
            if (item == null)
            {
                Debug.LogWarning($"Item not found at path: {package.ItemGUID}");
            }
            
            var so = ScriptableObject.CreateInstance<SO_Package>();
            so.Id = package.Id;
            so.PackageData = package.PackageData;
            so.Item = item;
            return so;
        }
        
        public static List<Package> ToPackageList(List<SO_Package> soPackages)
        {
            var result = new List<Package>();
            foreach (var so in soPackages)
            {
                result.Add(ToPackage(so));
            }
            return result;
        }
        
        public static List<SO_Package> ToScriptableObjectList(IEnumerable<Package> packages)
        {
            var result = new List<SO_Package>();
            foreach (var package in packages)
            {
                result.Add(ToScriptableObject(package));
            }
            return result;
        }
        
        public static SO_Package FindById(List<SO_Package> list, int id)
        {
            return list.Find(p => p.Id == id);
        }
    }
}