using System.Collections.Generic;
using CarePackage.Delivery;

namespace CarePackage.Persistance
{
    [System.Serializable]
    public class GameData
    {
        public string[] items;
        public string[] unacceptedItems;
        public Package[] deliveries;
        public Package currentDelivery;
        public List<Package> checkedSelectables;
        public List<int> randomNumbers;
        public long lastUpdated;
        public int money;
        public int requiredMoney;
        public int day;
        public float famARelationship;
        public bool survived;
        public bool doneTutorial;

        public GameData()
        {
            // initialize default values
        }
    }
}