using System.Collections.Generic;
using CarePackage.Delivery;

namespace CarePackage.Persistance
{
    [System.Serializable]
    public class GameData
    {
        public long lastUpdated;
        public string[] items;
        public string[] unacceptedItems;
        public Package[] deliveries;
        public Package currentDelivery;
        public List<int> randomNumbers;
        public int money;
        public int day;
        public bool survived;
        
        public float famARelationship;

        public GameData()
        {
            // initialize default values
        }
    }
}