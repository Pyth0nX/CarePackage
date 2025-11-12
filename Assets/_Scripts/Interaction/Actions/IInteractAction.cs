using System;
using CarePackage.Main;
using UnityEngine;

namespace CarePackage.Interaction
{
    public interface IInteractAction
    {
        void PerformAction(PlayerState interactingPlayer, GameObject interactingObject);
        bool ConditionMet(PlayerState interactingPlayer, GameObject interactingObject) { return true; }
    }

    public interface IInteractEvents
    {
        public event System.Action OnInteracted;
        public event System.Action OnInteractionComplete;
    }

    [System.Serializable]
    public abstract class InteractableWithEvents : IInteractEvents, IInteractAction
    {
        public event Action OnInteracted;
        public event Action OnInteractionComplete;

        private IInteractAction _action;

        public InteractableWithEvents(IInteractAction action)
        {
            _action = action;
        }
        
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            OnInteracted?.Invoke();
            _action.PerformAction(interactingPlayer, interactingObject);
        }
    }
}