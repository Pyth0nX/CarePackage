using CarePackage.Delivery;
using UnityEngine;

namespace CarePackage.Persistance
{
    [System.Serializable]
    public class GameData
    {
        public long lastUpdated;
        public SO_Item[] items;
        public SO_Item[] unacceptedItems;
        public IDeliverable[] deliveries;
        public IDeliverable currentDelivery;

        public GameData()
        {
            // initialize default values
        }
    }
}