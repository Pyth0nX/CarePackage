using CarePackage.Main;
using SerializeReferenceEditor;
using UnityEngine;

namespace CarePackage.Interaction
{
    public interface IPickup
    {
        Vector3 Offset { get; }
        GameObject OwningObject { get; set; }
        IPickupExtension[] ExtendedLogic { get; set; }
        
        void OnPickedUp(PlayerState interactingPlayer);
        void OnDropped(PlayerState interactingPlayer);
    }

    [System.Serializable]
    public class Pickup : IPickup
    {
        [SerializeField] private Vector3 offset;
        [SerializeField] private GameObject owningObject;
        [SerializeReference, SR] private IPickupExtension[] extendedLogic;
        
        public Vector3 Offset { get => offset; set => offset = value; }
        public GameObject OwningObject { get => owningObject; set => owningObject = value; }
        public IPickupExtension[] ExtendedLogic { get => extendedLogic; set => extendedLogic = value; }

        public Pickup()
        {
            offset = Vector3.zero;
            owningObject = null;
            ExtendedLogic = null;
        }

        public Pickup(Vector3 inOffset, GameObject inOwningObject)
        {
            Offset = inOffset;
            OwningObject = inOwningObject;
        }
        
        public Pickup(Vector3 inOffset, GameObject inOwningObject, IPickupExtension[] inPickupExtensions) : this(inOffset, inOwningObject)
        {
            ExtendedLogic = inPickupExtensions;
        }

        public void OnPickedUp(PlayerState interactingPlayer)
        {
            //if (owningObject != null) owningObject.GetComponent<Collider>().enabled = false;
            if (ExtendedLogic == null) return;
            foreach (var extendedPickup in ExtendedLogic)
            {
                extendedPickup.ExtendedPickUp(interactingPlayer);
            }
        }

        public void OnDropped(PlayerState interactingPlayer)
        {
            if (owningObject != null) owningObject.GetComponent<Collider>().enabled = true;
            if (ExtendedLogic == null) return;
            foreach (var extendedPickup in ExtendedLogic)
            {
                extendedPickup.ExtendedDropped(interactingPlayer);
            }
        }
    }
    
    public interface IPickupExtension
    {
        public void ExtendedPickUp(PlayerState interactingPlayer);
        public void ExtendedDropped(PlayerState interactingPlayer);
    }
}