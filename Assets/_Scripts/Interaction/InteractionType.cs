using System;
using UnityEngine.EventSystems;
using UnityEngine;

namespace CarePackage.Interaction
{
    [System.Serializable]
    public enum EInteractionType
    {
        Active, // interaction happening when pressing the interaction key
        Passive, // interaction happening when overlapping two objects
        Clicked, // interaction happening when clicked in UI or on the object with mouse pointer
    }

    public interface IInteractionActivationType
    {
        public void RaiseInteraction();
    }

    public abstract class InteractionActivationBase : IInteractionActivationType, IDisposable
    {
        private readonly Interactable _owner;
        
        protected InteractionActivationBase(Interactable inOwner)
        {
            _owner = inOwner;
        }
        
        public void RaiseInteraction()
        {
            _owner.Interact();
        }

        public void Dispose()
        {
            //OnDestruct();
        }

        protected virtual void OnDestruct() {}
    }

    public class InteractionOnClicked : InteractionActivationBase
    {
        private Clickable _clickable;
        
        public InteractionOnClicked(Interactable inOwner) : base(inOwner)
        {
            _clickable = inOwner.gameObject.AddComponent<Clickable>();
            _clickable.OnClicked += RaiseInteraction;
        }

        protected override void OnDestruct()
        {
            if (_clickable != null)
            {
                _clickable.OnClicked -= RaiseInteraction;
                UnityEngine.Object.DestroyImmediate(_clickable);
            }
            base.OnDestruct();
        }
    }

    public class Clickable : MonoBehaviour, IPointerDownHandler
    {
        public event System.Action OnClicked;
        
        public void OnPointerDown(PointerEventData eventData) => OnClicked?.Invoke();
        private void OnMouseDown() => OnClicked?.Invoke();
    }
    
    public class InteractionOnTriggered : InteractionActivationBase
    {
        private Triggerable _triggerable;
        
        public InteractionOnTriggered(Interactable inOwner) : base(inOwner)
        {
            _triggerable = inOwner.gameObject.AddComponent<Triggerable>();
            _triggerable.Initialize(inOwner, RaiseInteraction);
        }

        override protected void OnDestruct()
        {
            GameObject.DestroyImmediate(_triggerable);
        }
    }
    
    public class Triggerable : MonoBehaviour
    {
        private Interactable _owner;
        private System.Action _raise;
        
        public void Initialize(Interactable owner, System.Action raise)
        {
            _owner = owner;
            _raise = raise;
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((_owner.Layer.value & (1 << other.gameObject.layer)) != 0)
            {
                _raise?.Invoke();
            }
        }
    }
    
    public class InteractionOnInteracted : InteractionActivationBase
    {
        public InteractionOnInteracted(Interactable inOwner) : base(inOwner)
        {
            
        }
    }
}