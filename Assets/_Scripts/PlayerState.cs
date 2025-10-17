using CarePackage.Interaction;
using CarePackage.Job;
using UnityEngine;

namespace CarePackage.Main
{
    public class PlayerState : MonoBehaviour
    {
        [SerializeField] private Transform pickup;
        
        // private Components
        private JobManager _jobManager;
        private InteractionComponent _interactionComponent;
        
        public JobManager JobManager => _jobManager;
        public InteractionComponent InteractionComponent => _interactionComponent;

        private void Awake()
        {
            _jobManager = GetComponent<JobManager>();
            _interactionComponent = GetComponent<InteractionComponent>();
        }

        public void Pickup(GameObject objectToPickup)
        {
            objectToPickup.transform.SetParent(pickup);
            objectToPickup.transform.localPosition = Vector3.zero;
            objectToPickup.transform.localPosition += new Vector3(0, -.65f, 0);
            objectToPickup.transform.localRotation = Quaternion.identity;
        }
    }
}
