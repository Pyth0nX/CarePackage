using System.Collections.Generic;
using CarePackage.Persistance;
using System.Linq;
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
            items = loadData.items.ToList();
            unacceptedItems = loadData.unacceptedItems.ToList();
        }

        public void SaveData(GameData saveData)
        {
            saveData.items = items.ToArray();
            saveData.unacceptedItems = unacceptedItems.ToArray();
        }
    }
}