using CarePackage.Main;
using UnityEngine;

namespace CarePackage.Interaction
{
    public class InteractionComponent : MonoBehaviour
    {
        [SerializeField] private LayerMask interactionLayer;
        
        private IInteractable _interactable;
        private MonoBehaviour owner;
        
        public bool ValidInteraction() => _interactable != null;
        public bool IsPassive => _interactable.Type == InteractionType.Passive;
        public bool IsActive => _interactable.Type == InteractionType.Active;

        public void SetInteractable(IInteractable interactable)
        {
            _interactable = interactable;
        }

        /*
        private void OnTriggerEnter(Collider other)
        {/*
            if ((interactionLayer.value & (1 << gameObject.layer)) == 0)
            {
                Debug.LogWarning($"[Interactable] Layer mismatch: {gameObject.name} is on layer {gameObject.layer}, not in {interactionLayer}");
                return;
            }

            if (other.gameObject.TryGetComponent<IInteractable>(out _interactable))
            {
                if (IsPassive)
                {
                    Debug.LogWarning($"[Interactable] Passive interaction: {gameObject.name}");
                    TryInteract(owner as PlayerState);
                }
            }
        }*/

        private void OnTriggerExit(Collider other)
        {
            if ((interactionLayer.value & (1 << gameObject.layer)) == 0)
            {
                Debug.LogWarning($"[Interactable] Layer mismatch: {gameObject.name} is on layer {gameObject.layer}, not in {interactionLayer}");
                return;
            }
            _interactable = null;
        }

        public void TryInteract()
        {
            if (_interactable == null) return;
            _interactable.Interact(owner as PlayerState);
            _interactable = null;
        }
    }
}