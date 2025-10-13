using System;
using CarePackage.Main;
using SerializeReferenceEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CarePackage.Interaction
{
    public class Interactable : MonoBehaviour, IInteractable, IPointerDownHandler
    {
        [SerializeField] private InteractionType interactionType;
        [SerializeField] private LayerMask interactionLayer;
        [SerializeReference, SR] private InteractAction interactAction;
        
        public void Interact(PlayerState interactingPlayer)
        {
            if (interactAction == null) return;
            Debug.Log($"[Interactable] Interacted {this.name} {Type}");
            interactAction.PerformAction(interactingPlayer, gameObject);
        }
        
        public InteractionType Type {  get => interactionType; }
        
        public bool Clickable => interactionType == InteractionType.Clicked;
        public bool Passive => interactionType == InteractionType.Passive;
        
        private void OnMouseDown()
        {
            Debug.Log($"[Interactable OnMouseDown] Triggered {this.name} {Type}");/*
            if ((interactionLayer.value & (1 << gameObject.layer)) == 0)
            {
                Debug.LogWarning($"[Interactable] Layer mismatch: {gameObject.name} is on layer {gameObject.layer}, not in {interactionLayer}");
                return;
            }*/
            if (!Clickable) return;
            Interact(GameManager.Instance.Player);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log($"[Interactable OnPointerDown] Triggered {this.name} {Type}");
            if ((interactionLayer.value & (1 << gameObject.layer)) == 0)
            {
                Debug.LogWarning($"[Interactable] Layer mismatch: {gameObject.name} is on layer {gameObject.layer}, not in {interactionLayer}");
                return;
            }
            if (!Clickable) return;
            Interact(GameManager.Instance.Player);
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[Interactable OnTrigger] Triggered {this.name} {Type}");
            if ((interactionLayer.value & (1 << gameObject.layer)) == 0)
            {
                Debug.LogWarning($"[Interactable] Layer mismatch: {gameObject.name} is on layer {gameObject.layer}, not in {interactionLayer}");
                return;
            }

            if (other.TryGetComponent<InteractionComponent>(out var interactionComponent))
            {
                interactionComponent.SetInteractable(this);
                if (!Passive) return;
                interactionComponent.TryInteract();
            }
        }
    }
}