using NUnit.Framework;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
namespace OpenDialogue
{
    public class Inventory : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }
        public Item[] items = new Item[28];
        public InventoryItem[] inventoryItems = new InventoryItem[28];
        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < items.Length; i++)
            {
                Item item = items[i];
                if (item != new Item(string.Empty, null, -1))
                {
                    inventoryItems[i].image.sprite = item.image;
                    inventoryItems[i].itemName.text = item.name;
                }
            }
        }
        public void Add(Item item)
        {
            List<Item> editable = items.ToList<Item>();
            List<Item> toRemove = new List<Item>();
            foreach (Item i in editable)
            {
                if (i.id == 0)
                {
                    toRemove.Add(i);
                    Debug.Log("removed null");
                }
            }
            foreach (Item i in toRemove)
            {
                editable.Remove(i);
            }
            editable.Add(item);
            while (editable.Count < inventoryItems.Length - 1)
            {
                editable.Add(new Item(string.Empty, null, 0));
            }
            items = editable.ToArray();
        }
        public void Remove(Item item)
        {
            if (items.Contains(item))
            {
                List<Item> editable = items.ToList<Item>();
                List<Item> toRemove = new List<Item>();
                foreach (Item i in editable)
                {
                    if (i == null)
                    {
                        toRemove.Add(i);
                    }
                }
                foreach (Item i in toRemove)
                {
                    editable.Remove(i);
                }
                editable.Remove(item);
                for (int i = 0; i < items.Length; i++)
                {
                    if (i >= editable.Count)
                    {
                        editable.Add(null);
                    }
                }
                items = editable.ToArray();
            }
            else
            {
                Debug.Log("targeted item not in inventory");
            }

        }
    }
    [System.Serializable]
    public class Item
    {
        public string name;
        public Sprite image;
        public int id;
        public Item(string setName, Sprite setImage, int setId)
        {
            name = setName;
            image = setImage;
            id = setId;
        }
    }
}
