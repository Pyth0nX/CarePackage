using CarePackage.Interaction;
using CarePackage.Job;
using UnityEngine;

namespace CarePackage.Main
{
    public class PlayerState : MonoBehaviour
    {
        [SerializeField] private Transform pickupLocation;
        
        // private Components
        private JobManager _jobManager;
        private InteractionComponent _interactionComponent;

        private GameObject _pickup;
        
        public JobManager JobManager => _jobManager;
        public InteractionComponent InteractionComponent => _interactionComponent;
        public bool IsPickupValid => _pickup != null;

        private void Awake()
        {
            _jobManager = GetComponent<JobManager>();
            _interactionComponent = GetComponent<InteractionComponent>();
        }

        public void DropPickup()
        {
            if (!IsPickupValid) return;
            Drop(_pickup);
        }

        public void Drop(GameObject objectToDrop)
        {
            objectToDrop.transform.SetParent(null);
        }

        public void Pickup(GameObject objectToPickup)
        {
            objectToPickup.transform.SetParent(pickupLocation);
            SetPickup(objectToPickup);
            objectToPickup.transform.localPosition = Vector3.zero;
            objectToPickup.transform.localPosition += new Vector3(0, -.65f, 0);
            objectToPickup.transform.localRotation = Quaternion.identity;
        }

        public void SetPickup(GameObject pickedupObject)
        {
            _pickup = pickedupObject;
        }
    }
}
