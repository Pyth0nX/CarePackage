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
        
        private InteractionComponent _interactionComponent;
        
        private void Start()
        {
            _interactionComponent = GameManager.Instance.Player.InteractionComponent;
        }

        private void OnEnable()
        {
            if (interactAction is IActivatable activatable)
            {
                activatable.OnEnable();
            }
        }

        private void OnDisable()
        {
            if (interactAction is IActivatable activatable)
            {
                activatable.OnDisable();
            }
        }

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
            /*
                if ((interactionLayer.value & (1 << gameObject.layer)) == 0)
                {
                    Debug.LogWarning($"[Interactable] Layer mismatch: {gameObject.name} is on layer {gameObject.layer}, not in {interactionLayer}");
                    return;
                }*/
            if (!Clickable) return;
            Debug.Log($"[Interactable OnMouseDown] Triggered {this.name} {Type}");
            if (_interactionComponent == null) return;
            _interactionComponent.SetInteractable(this);
            _interactionComponent.TryInteract();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            /*
                if ((interactionLayer.value & (1 << gameObject.layer)) == 0)
                {
                    Debug.LogWarning($"[Interactable] Layer mismatch: {gameObject.name} is on layer {gameObject.layer}, not in {interactionLayer}");
                    return;
                }*/
            Debug.Log($"[Interactable OnPointerDown] Triggered {this.name} {Type}");
            if (!Clickable) return;
            if (_interactionComponent == null) return;
            _interactionComponent.SetInteractable(this);
            _interactionComponent.TryInteract(); /*
        Interact(GameManager.Instance.Player);
        _interactionComponent.SetInteractable(this);
        _interactionComponent.TryInteract();*/
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[Interactable OnTrigger] Triggered {this.name} {Type}");/*
            if ((interactionLayer.value & (1 << gameObject.layer)) == 0)
            {
                Debug.LogWarning($"[Interactable] Layer mismatch: {gameObject.name} is on layer {gameObject.layer}, not in {interactionLayer}");
                return;
            }*/
            
            if (other.transform.root.TryGetComponent<InteractionComponent>(out _interactionComponent))
            {
                Debug.Log($"[Interactable OnTrigger] Triggered {this.name} {Type} with {other.name}");
                _interactionComponent.SetInteractable(this);
                if (!Passive) return;
                _interactionComponent.TryInteract();
            }
            else
            {
                Debug.Log($"[Interactable OnTrigger] could not find a Interactable component for {other.transform.root.name} {other.TryGetComponent<InteractionComponent>(out InteractionComponent inter)}");
            }
        }
    }
}