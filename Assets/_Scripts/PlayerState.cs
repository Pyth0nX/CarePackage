using CarePackage.Interaction;
using CarePackage.Delivery;
using UnityEngine;

namespace CarePackage.Main
{
    public class PlayerState : MonoBehaviour
    {
        [SerializeField] private Transform pickupLocation;
        
        [Header("PickupLogic")]
        [SerializeField] private float breakForce = 800f;
        [SerializeField] private bool breakIfTooMuchForce;
        
        // public getters
        public DeliveryManager DeliveryManager => _deliveryManager;
        public InteractionComponent InteractionComponent =>
            _switchMode.ActivePlayer.GetComponent<InteractionComponent>();
        public Inventory Inventory => _inventory;
        public ModeSwitcher SwitchMode => _switchMode;
        public GameObject ActivePlayer => _switchMode.ActivePlayer;
        public GameObject PickupObject => _pickup.OwningObject;
        public bool IsPickupValid => _pickup != null;

        // private Components
        private DeliveryManager _deliveryManager;
        private Inventory _inventory;
        private ModeSwitcher _switchMode;

        [SerializeReference, SerializeReferenceEditor.SR] private IPickup _pickup;
        private ConfigurableJoint _pickupJoint;
        private Rigidbody _pickupRigidbody;

        private Vector3 _pickupOffsetAnchor = new(0, 0, -0.25f);
        private float _pickupDistanceOffset = 1;
        private float _minDistance = -0.25f;
        private float _maxDistance = -1f;

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
                Task.TaskManager.PushTaskUpdate(new Task.Task("Hello traveller " + 1));
            }
        }

        public void Pickup(IPickup objectToPickup, GameObject objectOfPickup)
        {
            if (IsPickupValid) DropPickup();
            SetPickup(objectToPickup, objectOfPickup);
            if (!IsPickupValid) return;
            
            AttachPickupWithJoint();
            
            _pickup.OnPickedUp(this);
        }

        private void AttachPickupWithJoint()
        {
            _pickupRigidbody = _pickup.OwningObject.GetComponent<Rigidbody>();

            _pickupJoint = pickupLocation.gameObject.AddComponent<ConfigurableJoint>();
            _pickupJoint.connectedBody = _pickupRigidbody;
            _pickupJoint.autoConfigureConnectedAnchor = false;
            
            Vector3 worldOffset = _pickup.OwningObject.transform.position - pickupLocation.position;
            _pickupOffsetAnchor = pickupLocation.InverseTransformPoint(_pickup.OwningObject.transform.position);
            
            _pickupJoint.anchor = _pickupOffsetAnchor;
            _pickupJoint.connectedAnchor = Vector3.zero;
            
            _pickupJoint.xMotion = ConfigurableJointMotion.Limited; // locked
            _pickupJoint.yMotion = ConfigurableJointMotion.Limited;
            _pickupJoint.zMotion = ConfigurableJointMotion.Limited;

            _pickupJoint.angularXMotion = ConfigurableJointMotion.Limited;
            _pickupJoint.angularYMotion = ConfigurableJointMotion.Limited;
            _pickupJoint.angularZMotion = ConfigurableJointMotion.Limited;
            
            JointDrive drive = new JointDrive { positionSpring = 500, positionDamper = 50, maximumForce = Mathf.Infinity };
            _pickupJoint.xDrive = drive;
            _pickupJoint.yDrive = drive;
            _pickupJoint.zDrive = drive;
            
            SoftJointLimit limit = new SoftJointLimit { limit = 20f }; // degrees of swing
            _pickupJoint.highAngularXLimit = limit;
            _pickupJoint.lowAngularXLimit = limit;
            _pickupJoint.angularYLimit = limit;
            _pickupJoint.angularZLimit = limit;

            if (breakIfTooMuchForce)
            {
                _pickupJoint.breakForce = breakForce;
                _pickupJoint.breakTorque = breakForce;
            }
        }

        public void SetPickup(IPickup inPickeup, GameObject pickupObject)
        {
            inPickeup.OwningObject = pickupObject;
            _pickup = inPickeup;
        }

        public void DropPickup()
        {
            if (!IsPickupValid) return;
            Drop(_pickup);
        }

        public void Drop(IPickup pickupToDrop)
        {
            pickupToDrop.OnDropped(this);
            DetachPickupJoint();
            
            DeliveryManager.SetCurrentHeldDelivery(null);
            SetPickup(pickupToDrop, null);
            _pickup = null;
        }
        
        public void DetachPickupJoint()
        {
            if (!_pickupJoint) return;
            Destroy(_pickupJoint);
        }

        public void LaunchPickup(float heldDuration)
        {
            var launchPickupAction = new Interaction.Miscellaneous.LaunchPickupAction(PickupObject, heldDuration);
            launchPickupAction.PerformAction(this, PickupObject);
        }

        public void ChangePickupDistance(float incomingValue)
        {
            _pickupDistanceOffset = Mathf.Clamp(_pickupDistanceOffset - incomingValue, _maxDistance, _minDistance);
            _pickupOffsetAnchor = new Vector3(0, 0, _pickupDistanceOffset);
            
            if (!IsPickupValid) return;
            _pickupJoint.anchor = _pickupOffsetAnchor;
        }
    }
}
