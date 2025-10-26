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
        public float requiredMoney;
        public int money;
        public bool survived;

        public GameData()
        {
            // initialize default values
        }
    }
}