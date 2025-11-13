using System.Collections.Generic;
using CarePackage.Interaction.Miscellaneous;
using CarePackage.Delivery;
using CarePackage.Main;
using CarePackage.Utilities;
using UnityEngine;
using System;

namespace CarePackage.Interaction.Car
{
    [Serializable]
    public class EnterCarAction : IInteractAction
    {
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            var switchMode = interactingPlayer.SwitchMode;
            switchMode.EnterCarMode(interactingObject.transform);
        }
    }
    
    [Serializable]
    public class ExitCarAction : IInteractAction
    {
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            var switchMode = interactingPlayer.SwitchMode;
            switchMode.EnterFirstPersonMode(interactingObject.transform.root);
        }
    }
    
    [Serializable]
    public class OpenCarAction : IInteractAction, IActivatable
    {
        private static readonly int AnimOpen = Animator.StringToHash("isOpen");
        private static readonly int AnimSpeed = Animator.StringToHash("animSpeed");
        private static readonly int AnimOverride = Animator.StringToHash("overrideOpen");
        
        
        [SerializeField] private Animator animator;
        
        private bool _open;
        
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            _open = !_open;
            ToggleCarTrunk();
        }

        public void OnEnable()
        {
            animator.SetBool(AnimOverride, true);
            ToggleCarTrunk();
            PrimeTween.Tween.Delay(1.5f).OnComplete(() => { animator.SetBool(AnimOverride, false); });
        }

        public void OnDisable() {}

        private void ToggleCarTrunk()
        {
            animator.SetBool(AnimOpen, _open);
        }
    }
    
    [Serializable]
    public class TryStartDayWithCarAction : IInteractAction
    {
        [SerializeField] private string sceneName;
        [SerializeField] private Transform packageContainer;
        
        private int requiredDeliveries => GameManager.Instance.Player.DeliveryManager.GetDeliveryQuotas;

        private bool HasTheRequiredPackages => _hasInitedRequiredPackages &&
                                               DeliveryUitilities.DoesListContainPackages(_collectedDeliveries, _requiredPackages);
        
        private List<Package> _collectedDeliveries = new ();
        private List<Vector3> _packagePositions = new();
        private Package[] _requiredPackages;
        private bool _positionsCached;
        private bool _hasInitedRequiredPackages;
        
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (!_positionsCached)
            {
                _packagePositions = PositionUtilities.GenerateStrict2x3Grid(packageContainer.position, requiredDeliveries, .8f);
                _positionsCached = true;
            }

            if (!_hasInitedRequiredPackages)
            {
                _requiredPackages = interactingPlayer.DeliveryManager.RequiredDeliveries;
                _hasInitedRequiredPackages = true;
            }
            
            if (HasTheRequiredPackages && !interactingPlayer.IsPickupValid)
            {
                var switchSceneAction = new SwitchSceneAction(sceneName);
                switchSceneAction.PerformAction(interactingPlayer, interactingObject);
            }
            
            if (interactingPlayer.DeliveryManager == null) return;
            if (interactingPlayer.DeliveryManager.CurrentDelivery == null)
            {
                Debug.Log("[DeliveryManager.CurrentDelivery] You do not have a delivery to start the day");
                return;
            }
            
            var heldDelivery = interactingPlayer.DeliveryManager.CurrentDelivery;
            if (heldDelivery != null) _collectedDeliveries.Add(heldDelivery);
            var package = interactingPlayer.PickupObject;
            interactingPlayer.DropPickup();
            package.transform.localPosition = _packagePositions[_collectedDeliveries.Count - 1];
            package.transform.SetParent(packageContainer, true);
        }

        public bool ConditionMet(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (_collectedDeliveries.Count >= requiredDeliveries) return true;
            return false;
        }
    }
}
