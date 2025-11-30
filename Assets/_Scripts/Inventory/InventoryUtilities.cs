using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarePackage.Delivery
{
    public static class InventoryUtilities
    {
        public static SO_Item LoadItem(string GUIDPath)
        {
            var loadedItem = Resources.Load<SO_Item>(GUIDPath);
            if (loadedItem != null) return loadedItem;
            Debug.LogWarning($"Item not found at path: {GUIDPath}");
            return loadedItem;
        }

        public static List<SO_Item> LoadItems(List<string> GUIDPaths)
        {
            var loadedItems = new List<SO_Item>();
            foreach (var guid in GUIDPaths)
            {
                var loadedItem = LoadItem(guid);
                if (loadedItem != null) loadedItems.Add(loadedItem);
            }
            Debug.LogWarning($"Some items may not have loaded. Count: {loadedItems.Count}/{GUIDPaths.Count}");
            return loadedItems;
        }

        public static string GetItemGUID(SO_Item item)
        {
            var itemGUID = item != null ? item.GUID : String.Empty;
            return itemGUID;
        }

        public static List<string> GetItemGUIDs(List<SO_Item> items)
        {
            var itemGUIDs = new List<string>();
            foreach (var item in items)
            {
                var itemGUID = GetItemGUID(item);
                itemGUIDs.Add(itemGUID);
            }
            return itemGUIDs;
        }


    }
}