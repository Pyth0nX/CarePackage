using System;
using CarePackage.Main;
using UnityEngine;

namespace CarePackage.Interaction
{
    public class Interactable : MonoBehaviour, IInteractable
    {
        [SerializeField] private EInteractionType interactionType;
        [SerializeField] private LayerMask interactionLayer;
        [SerializeReference, SerializeReferenceEditor.SR] private IInteractAction interactAction;
        [SerializeField] private string interactText;
        [SerializeField] private bool showMessage;
        [SerializeField] private bool debug;
        
        public IInteractAction InteractAction { get => interactAction; set => interactAction = value; }
        public IInteractionActivationType ActivationType => _activationType;
        public LayerMask Layer => interactionLayer;
        public EInteractionType Type => interactionType;
        public string InteractMessage => interactText;
        public bool ShowMessage => showMessage;
        
        private IInteractionActivationType _activationType;
        private Outline _outline;
#if UNITY_EDITOR
        private EInteractionType _LastType;
#endif
        
        public event System.Action<Interactable> OnInteracted;
        public event System.Action<Interactable> OnInteractionFinished;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            switch (interactionType)
            {
                case EInteractionType.Active:
                    _activationType = new InteractionOnInteracted(this);
                    break;
                case EInteractionType.Passive:
                    _activationType = new InteractionOnTriggered(this);
                    break;
                case EInteractionType.Clicked:
                    _activationType = new InteractionOnClicked(this);
                    break;
                default:
                    break;
            }
            _LastType = interactionType;
        }

        private void Start()
        {
            if (interactionLayer == LayerMask.NameToLayer("Default")) interactionLayer = LayerMask.GetMask("Interaction");
            _outline = GetComponent<Outline>();
            
            if (_outline == null) return;
            _outline.enabled = false;
        }
/*
        private void OnValidate()
        {
            if (interactionType == _LastType) return;
            
            if (_activationType is IDisposable disposable)
                disposable.Dispose();
            
            Init();
        }*/

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
        
        public void Interact()
        {
            if (interactAction == null) return;
            interactAction.PerformAction(GameManager.Instance.Player, gameObject);
            if (debug) Debug.Log($"[Interactable] {interactionType} Interacted");
            OnInteracted?.Invoke(this);
            OnInteractionFinished?.Invoke(this);
        }

        public void OnHovered(bool toggle)
        {
            if (_outline == null) return;
            _outline.enabled = toggle;
        }
    }
}