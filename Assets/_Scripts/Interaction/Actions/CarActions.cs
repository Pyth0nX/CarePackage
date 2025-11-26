using System.Collections.Generic;
using CarePackage.Interaction.Miscellaneous;
using CarePackage.Delivery;
using CarePackage.Main;
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
        {/*
            animator.SetBool(AnimOverride, true);
            ToggleCarTrunk();
            PrimeTween.Tween.Delay(1.5f).OnComplete(() => { animator.SetBool(AnimOverride, false); });*/
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
        [SerializeField] private Transform packageContainer;
        
        private int requiredDeliveries => GameManager.Instance.Player.DeliveryManager.GetDeliveryQuotas;

        private bool HasTheRequiredPackages() => _hasInitedRequiredPackages && DeliveryUitilities.DoesListContainPackages(_collectedDeliveries, _requiredPackages);
        
        private List<Package> _collectedDeliveries = new ();
        private Package[] _requiredPackages;
        private bool _positionsCached;
        private bool _hasInitedRequiredPackages;
        
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (!_hasInitedRequiredPackages)
            {
                _requiredPackages = interactingPlayer.DeliveryManager.RequiredDeliveries;
                _hasInitedRequiredPackages = true;
            }

            _collectedDeliveries = interactingPlayer.DeliveryManager.Deliveries;
            
            if (HasTheRequiredPackages() && !interactingPlayer.IsPickupValid)
            {
                Debug.Log("Going to new Scene");
                var switchSceneAction = new SwitchSceneAction(ECarePackageScenes.NeighbourHood);
                switchSceneAction.PerformAction(interactingPlayer, interactingObject);
            }
            
            if (interactingPlayer.DeliveryManager == null) return;
        }

        public bool ConditionMet(PlayerState interactingPlayer, GameObject interactingObject)
        {
            var count = interactingPlayer.DeliveryManager.Deliveries.Count;
            if (count >= requiredDeliveries) return true;
            return false;
        }
    }
}
