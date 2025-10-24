using CarePackage.Delivery;

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
        public float requiredMoney;
        public int money;
        public bool lost;

        public GameData()
        {
            // initialize default values
        }
    }
}