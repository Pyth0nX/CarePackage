using CarePackage.Interaction;
using CarePackage.Delivery;
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

        private GameObject _pickup;

        public DeliveryManager DeliveryManager => _deliveryManager;

        public InteractionComponent InteractionComponent =>
            _switchMode.ActivePlayer.GetComponent<InteractionComponent>();

        public Inventory Inventory => _inventory;
        public ModeSwitcher SwitchMode => _switchMode;
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

        public void DropPickup()
        {
            if (!IsPickupValid) return;
            Drop(_pickup);
        }

        public void Drop(GameObject objectToDrop)
        {
            objectToDrop.transform.SetParent(null);
        }
    }
}
