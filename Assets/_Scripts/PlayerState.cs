using CarePackage.Interaction;
using CarePackage.Delivery;
using UnityEngine;

namespace CarePackage.Main
{
    public class PlayerState : MonoBehaviour
    {
        [SerializeField] private PlayerModeData modeData;
        [SerializeField] private Transform pickupLocation;
        
        // private Components
        private DeliveryManager _deliveryManager;
        private InteractionComponent _interactionComponent;
        private Inventory _inventory;
        private ModeSwitcher _switchMode;
        
        private GameObject _pickup;
        
        public DeliveryManager DeliveryManager => _deliveryManager;
        public InteractionComponent InteractionComponent => _interactionComponent;
        public Inventory Inventory => _inventory;
        public ModeSwitcher SwitchMode => _switchMode;
        public bool IsPickupValid => _pickup != null;

        private void Awake()
        {
            _deliveryManager = GetComponent<DeliveryManager>();
            _interactionComponent = GetComponent<InteractionComponent>();
            _inventory = GetComponent<Inventory>();
            _switchMode = GetComponent<ModeSwitcher>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                SwitchPlayerMode();
            }
        }

        public void SwitchPlayerMode()
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
