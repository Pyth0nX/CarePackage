using System.Collections.Generic;
using CarePackage.Persistance;
using System.Linq;
using CarePackage.Delivery;
using UnityEngine;

namespace CarePackage.Main
{
    public class Inventory : MonoBehaviour, IDataPersistance
    {
        [SerializeField] private List<SO_Item> items = new();
        [SerializeField] private List<SO_Item> unacceptedItems = new();
        
        public List<SO_Item> GetItems() => items;
        public List<SO_Item> GetUnacceptedItems() => unacceptedItems;

        public void AddItem(SO_Item item)
        {
            items.Add(item);
        }

        public void AddUnacceptedItem(SO_Item item)
        {
            unacceptedItems.Add(item);
        }

        public void RemoveItem(SO_Item item)
        {
            items.Remove(item);
        }
        
        public SO_Item GetItemAtIndex(int index) => items[index];
        public SO_Item GetItemByGUID(string guid) => items.Find(i => i.GUID == guid);
        public SO_Item GetItem(SO_Item item) => items.Find(i => i == item);
        
        public SO_Item GetUnacceptedItemAtIndex(int index) => unacceptedItems[index];
        public SO_Item GetUnacceptedItemByGUID(string guid) => unacceptedItems.Find(i => i.GUID == guid);
        public SO_Item GetUnacceptedItem(SO_Item item) => unacceptedItems.Find(i => i == item);

        public void AcceptItem(SO_Item item)
        {
            if (!items.Contains(item) && unacceptedItems.Contains(item))
            {
                unacceptedItems.Remove(item);
                AddItem(item);
            }
        }

        public void LoadData(GameData loadData)
        {
            if (loadData.items != null) items = InventoryUtilities.LoadItems(loadData.items.ToList());
            if (loadData.unacceptedItems != null) unacceptedItems = InventoryUtilities.LoadItems(loadData.unacceptedItems.ToList());
        }

        public void SaveData(GameData saveData)
        {
            if (items != null) saveData.items = InventoryUtilities.GetItemGUIDs(items).ToArray();
            if (unacceptedItems != null) saveData.unacceptedItems = InventoryUtilities.GetItemGUIDs(unacceptedItems).ToArray();
        }
    }
}