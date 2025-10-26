using UnityEngine;

namespace CarePackage.Delivery
{
    [CreateAssetMenu(fileName = "Mail Letter", menuName = "CarePackage/Deliverable/Letter")]
    public class SO_Mail : ScriptableObject
    {
        public int Id;
        public int Pay;
        public int AddressToDeliver;
    }
}