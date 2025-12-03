using CarePackage.Main;
using SerializeReferenceEditor;
using UnityEngine;

namespace CarePackage.Interaction
{
    public interface IPickup
    {
        public Vector3 Offset { get; }
        public GameObject OwningObject { get; set; }
        public IPickupExtension[] ExtendedLogic { get; set; }
        public EPickupState PickupState { get; set; }
        
        void OnPickedUp(PlayerState interactingPlayer);
        void OnDropped(PlayerState interactingPlayer);
    }
    
    public enum EPickupState { Idle, PickedUp, Dropped }

    [System.Serializable]
    public class Pickup : IPickup
    {
        [SerializeField] private GameObject owningObject;
        [SerializeReference, SR] private IPickupExtension[] extendedLogic;
        [SerializeField] private Vector3 offset;
        [SerializeField] private EPickupState pickupState = EPickupState.Idle;
        
        public GameObject OwningObject { get => owningObject; set => owningObject = value; }
        public IPickupExtension[] ExtendedLogic { get => extendedLogic; set => extendedLogic = value; }
        public Vector3 Offset { get => offset; set => offset = value; }
        public EPickupState PickupState { get => pickupState; set => pickupState = value; }
        
        private Outline _pickUpOutline;

        public Pickup() : this(Vector3.zero, null, null) {}

        public Pickup(Vector3 inOffset, GameObject inOwningObject) : this(inOffset, inOwningObject, null) {}
        
        public Pickup(Vector3 inOffset, GameObject inOwningObject, IPickupExtension[] inPickupExtensions)
        {
            OwningObject = inOwningObject;
            Offset = inOffset;
            ExtendedLogic = inPickupExtensions;
            _pickUpOutline = inOwningObject != null ? inOwningObject.GetComponentInChildren<Outline>() : null;
        }

        public void OnPickedUp(PlayerState interactingPlayer)
        {
            if (ExtendedLogic == null) return;
            foreach (var extendedPickup in ExtendedLogic)
            {
                extendedPickup.ExtendedPickUp(interactingPlayer);
            }
            pickupState =  EPickupState.PickedUp;
            if (_pickUpOutline == null) return;
            _pickUpOutline.enabled = true;
        }

        public void OnDropped(PlayerState interactingPlayer)
        {
            if (owningObject != null) owningObject.GetComponent<Collider>().enabled = true;
            if (ExtendedLogic == null) return;
            foreach (var extendedPickup in ExtendedLogic)
            {
                extendedPickup.ExtendedDropped(interactingPlayer);
            }
            pickupState = EPickupState.Dropped;
            if (_pickUpOutline == null) return;
            _pickUpOutline.enabled = false;
        }
    }
    
    public interface IPickupExtension
    {
        public void ExtendedPickUp(PlayerState interactingPlayer);
        public void ExtendedDropped(PlayerState interactingPlayer);
    }
}