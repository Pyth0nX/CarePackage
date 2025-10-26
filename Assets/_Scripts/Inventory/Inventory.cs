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

        public void RemoveItem(SO_Item item)
        {
            items.Remove(item);
        }
        
        public SO_Item GetItem(int index) => items[index];
        
        public SO_Item GetUnacceptedItem(int index) => unacceptedItems[index];

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