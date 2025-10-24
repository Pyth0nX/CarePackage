using UnityEngine;

namespace CarePackage.Delivery
{
    [CreateAssetMenu(fileName = "Mail Letter", menuName = "CarePackage/Deliverable/Letter")]
    public class SO_Mail : IDeliverable
    {
        public int AddressToDeliver;
    }
}