using CarePackage.Main;
using SerializeReferenceEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CarePackage.Interaction
{
    public class Interactable : MonoBehaviour, IInteractable, IPointerDownHandler
    {
        [SerializeField] private InteractionType interactionType;
        [SerializeReference, SR] private InteractAction interactAction;
        [SerializeField] private LayerMask interactionLayer;
        
        public void Interact(PlayerState interactingPlayer)
        {
            if ((interactionLayer.value & (1 << gameObject.layer)) == 0)
            {
                Debug.LogWarning($"[Interactable] Layer mismatch: {gameObject.name} is on layer {gameObject.layer}, not in {interactionLayer}");
                return;
            }
            if (interactAction == null) return;
            interactAction.PerformAction(interactingPlayer, gameObject);
        }
        
        public InteractionType Type {  get => interactionType; }
        
        private bool Clickable => interactionType == InteractionType.Clicked;
        private bool Passive => interactionType == InteractionType.Passive;
        private bool Active => interactionType == InteractionType.Active;
        
        private void OnMouseDown()
        {
            if ((interactionLayer.value & (1 << gameObject.layer)) == 0)
            {
                Debug.LogWarning($"[Interactable] Layer mismatch: {gameObject.name} is on layer {gameObject.layer}, not in {interactionLayer}");
                return;
            }
            if (!Clickable) return;
            Interact(GameManager.Instance.Player);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if ((interactionLayer.value & (1 << gameObject.layer)) == 0)
            {
                Debug.LogWarning($"[Interactable] Layer mismatch: {gameObject.name} is on layer {gameObject.layer}, not in {interactionLayer}");
                return;
            }
            if (!Clickable) return;
            Interact(GameManager.Instance.Player);
        }
    }
}