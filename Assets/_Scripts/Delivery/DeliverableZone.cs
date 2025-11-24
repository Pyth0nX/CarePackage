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
        private BoxCollider _collider;
        
        public IInteractAction InteractLogic { get => interactLogic ; set => interactLogic = value; }
        
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
            if (pickup.PickupState != EPickupState.Dropped) return;
            
            //if (action is PackageAction packageAction && packageAction.AlreadyAdded) return;
            ExecuteLogic(pickupObject);
        }

        private void ExecuteLogic(GameObject interactedObject)
        {
            Debug.Log("Doing something to: " + interactedObject.name);
            interactLogic.PerformAction(GameManager.Instance.Player, interactedObject);
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.95f, 0f, 0.2f);
            Gizmos.matrix = base.transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }
    }
}