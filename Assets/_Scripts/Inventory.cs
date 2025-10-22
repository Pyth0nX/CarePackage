using System.Collections.Generic;
using UnityEngine;

namespace CarePackage.Main
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private List<GameObject> items = new();
        [SerializeField] private List<GameObject> unacceptedItems = new();
        
        public List<GameObject> GetItems() => items;
        public List<GameObject> GetUnacceptedItems() => unacceptedItems;

        public void AddItem(GameObject item)
        {
            items.Add(item);
        }

        public void RemoveItem(GameObject item)
        {
            items.Remove(item);
        }
        
        public GameObject GetItem(int index) => items[index];

        public void AcceptItem(GameObject item)
        {
            if (!items.Contains(item) && unacceptedItems.Contains(item))
            {
                unacceptedItems.Remove(item);
                AddItem(item);
            }
        }
    }
}
