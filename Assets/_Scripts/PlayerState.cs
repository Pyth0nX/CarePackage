using CarePackage.Interaction;
using CarePackage.Job;
using UnityEngine;

namespace CarePackage.Main
{
    public class PlayerState : MonoBehaviour
    {
        [SerializeField] private PlayerModeData modeData;
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                SwitchMode();
            }
        }

        public void SwitchMode()
        {
            var rb = GetComponent<Rigidbody>();
            var capsule =  GetComponent<CapsuleCollider>();
            
            if (modeData.car.activeSelf)
            {
                modeData.car.SetActive(false);
                modeData.carController.enabled = false;
                modeData.carCamera.SetActive(false);
                
                rb.mass = modeData.firstPersonMass;

                modeData.player.SetActive(true);
                modeData.playerController.enabled = true;
                
                capsule.enabled = true;

                return;
            }
            modeData.car.SetActive(true);
            modeData.carController.enabled = true;
            modeData.carCamera.SetActive(true);
            
            rb.mass = modeData.carMass;
            
            modeData.player.SetActive(false);
            modeData.playerController.enabled = false;
            capsule.enabled = false;
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

    [System.Serializable]
    public struct PlayerModeData
    { 
        public float firstPersonMass;
        public PlayerController playerController;
        public GameObject player;
        public float carMass;
        public PrometeoCarController carController;
        public GameObject car;
        public GameObject carCamera;
    }
}
