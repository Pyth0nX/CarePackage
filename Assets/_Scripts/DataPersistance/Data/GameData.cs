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
        public SO_Job[] deliveries;
        public SO_Job currentDelivery;

        public GameData()
        {
            // initialize default values
        }
    }
}