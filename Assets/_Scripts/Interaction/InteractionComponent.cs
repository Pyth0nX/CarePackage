using CarePackage.Main;
using UnityEngine;

namespace CarePackage.Interaction
{
    public class InteractionComponent : MonoBehaviour
    {
        [SerializeField] private LayerMask interactionLayer;
        
        private IInteractable _interactable;
        private GameObject owner;

        private void OnTriggerEnter(Collider other)
        {
            if ((interactionLayer.value & (1 << gameObject.layer)) == 0)
            {
                Debug.LogWarning($"[Interactable] Layer mismatch: {gameObject.name} is on layer {gameObject.layer}, not in {interactionLayer}");
                return;
            }

            if (other.gameObject.TryGetComponent<IInteractable>(out _interactable))
            {
                if (_interactable.Type == InteractionType.Passive)
                {
                    //_interactable.Interact(owner);
                    _interactable = null;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if ((interactionLayer.value & (1 << gameObject.layer)) == 0)
            {
                Debug.LogWarning($"[Interactable] Layer mismatch: {gameObject.name} is on layer {gameObject.layer}, not in {interactionLayer}");
                return;
            }
            
            _interactable = null;
        }

        public void TryInteract(PlayerState player)
        {
            if (_interactable == null) return;
            _interactable.Interact(player);
        }
    }
}