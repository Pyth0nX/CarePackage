using CarePackage.Interaction;
using CarePackage.Delivery;
using CarePackage.Interaction.Delivery;
using UnityEngine;

namespace CarePackage.Main
{
    public class PlayerState : MonoBehaviour
    {
        [SerializeField] private Transform pickupLocation;

        // private Components
        private DeliveryManager _deliveryManager;
        private Inventory _inventory;
        private ModeSwitcher _switchMode;
        private IPickup _pickup;

        public DeliveryManager DeliveryManager => _deliveryManager;
        public InteractionComponent InteractionComponent => _switchMode.ActivePlayer.GetComponent<InteractionComponent>();
        public Inventory Inventory => _inventory;
        public ModeSwitcher SwitchMode => _switchMode;
        public GameObject PickupObject => _pickup.OwningObject;
        public bool IsPickupValid => _pickup != null;

        private void Awake()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            _deliveryManager = GetComponent<DeliveryManager>();
            _inventory = GetComponent<Inventory>();
            _switchMode = GetComponent<ModeSwitcher>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {

            }
        }

        public GameObject ActivePlayer => _switchMode.ActivePlayer;
        
        public void Pickup(IPickup objectToPickup, GameObject objectOfPickup)
        {
            SetPickup(objectToPickup, objectOfPickup);
            
            _pickup.OwningObject.transform.SetParent(pickupLocation);
            _pickup.OwningObject.transform.localPosition = Vector3.zero;
            _pickup.OwningObject.transform.localPosition += _pickup.Offset;
            _pickup.OwningObject.transform.localRotation = Quaternion.identity;
            /*
            FixedJoint joint = pickupLocation.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = ActivePlayer.GetComponent<Rigidbody>();*/
            
            if (_pickup.OwningObject.TryGetComponent<Interactable>(out var interactable) 
                && interactable.InteractAction is PackageAction packageAction)
            {
                DeliveryManager.SetCurrentHeldDelivery(packageAction.Package);
            }
            
            _pickup.OnPickedUp(this);
        }

        public void SetPickup(IPickup pickedupObject, GameObject pickupObject)
        {
            pickedupObject.OwningObject = pickupObject;
            _pickup = pickedupObject;
        }

        public void DropPickup()
        {
            if (!IsPickupValid) return;
            Drop(_pickup);
        }

        public void Drop(IPickup objectToDrop)
        {
            objectToDrop.OwningObject.transform.SetParent(null);
            DeliveryManager.SetCurrentHeldDelivery(null);
            objectToDrop.OnDropped(this);
        }
    }
}
