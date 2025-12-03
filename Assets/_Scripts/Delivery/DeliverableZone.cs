using System;
using CarePackage.Interaction;
using CarePackage.Interaction.Delivery;
using CarePackage.Main;
using UnityEngine;

namespace CarePackage.Delivery
{
    [RequireComponent(typeof(BoxCollider))]
    public class DeliverableZone : MonoBehaviour
    {
        [SerializeField] private LayerMask layerMask;
        [SerializeReference, SerializeReferenceEditor.SR] private IInteractAction interactLogic;
        
        public IInteractAction InteractLogic { get => interactLogic ; set => interactLogic = value; }
        
        private BoxCollider _collider;
        
        private void Start()
        {
            layerMask = LayerMask.GetMask("Interaction");
            _collider = GetComponent<BoxCollider>();
            _collider.isTrigger = true;
            if (interactLogic == null) interactLogic = new ZoneReceivePackage();
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((layerMask.value & (1 << other.gameObject.layer)) == 0)
            {
                Debug.LogWarning($"[Interactable] Layer mismatch: {gameObject.name} is on layer {gameObject.layer}, not in {layerMask}");
                return;
            }
            
            var pickup = DeliveryUitilities.TryGetActionFromObjectNonRestrictive<IPickup>(other.gameObject);
            if (pickup == null) return;
            
            OnPickupStateChanged(pickup, pickup.PickupState.Value);
            pickup.PickupState.OnValueChanged += OnPickupStateChanged;
            /*
            var interactable = other.GetComponent<Interactable>();
            if (interactable == null) return;

            var action = interactable.InteractAction;
            if (action == null)
            {
                Debug.LogWarning($"[Interactable] {interactable.name} has no InteractAction assigned.");
                return;
            }

            var pickupObject = other.gameObject;

            if (action is not IPickup pickup) return;
            if (pickup.PickupState.Value == EPickupState.Idle) return;;*/
        }

        private void OnTriggerExit(Collider other)
        {
            var pickup = DeliveryUitilities.TryGetActionFromObjectNonRestrictive<IPickup>(other.gameObject);
            if (pickup == null) return;
            
            pickup.PickupState.OnValueChanged -= OnPickupStateChanged;
        }

        private void OnPickupStateChanged(IPickup pickup, EPickupState pickupState)
        {
            if (pickupState != EPickupState.Dropped) return;

            Debug.Log("Doing something to: " + pickup.OwningObject.name);
            interactLogic.PerformAction(GameManager.Instance.Player, pickup.OwningObject);
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.95f, 0f, 0.2f);
            Gizmos.matrix = base.transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }
    }
}