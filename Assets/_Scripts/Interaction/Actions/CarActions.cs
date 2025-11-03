using System;
using System.Collections.Generic;
using CarePackage.Interaction.Miscellaneous;
using CarePackage.Delivery;
using CarePackage.Main;
using UnityEngine;

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
    public class OpenCarAction : IInteractAction
    {
        [SerializeField] private GameObject carHoodClosed;
        [SerializeField] private GameObject carHoodOpen;

        private bool _opened;
        
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            _opened = !_opened;
            carHoodClosed.SetActive(!_opened);
            carHoodOpen.SetActive(_opened);
        }
    }
    
    [Serializable]
    public class TryStartDayWithCarAction : IInteractAction
    {
        [SerializeField] private string sceneName;
        [SerializeField] private Transform packageContainer;
        
        private int requiredDeliveries => GameManager.Instance.Player.DeliveryManager.GetDeliveryQuotas;
        private List<Package> _collectedDeliveries = new ();
        
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (interactingPlayer.DeliveryManager == null) return;
            if (interactingPlayer.DeliveryManager.CurrentDelivery == null)
            {
                Debug.Log("[DeliveryManager.CurrentDelivery] You do not have a delivery to start the day");
                return;
            }
            var heldDelivery = interactingPlayer.DeliveryManager.CurrentDelivery;
            if (heldDelivery != null) _collectedDeliveries.Add(heldDelivery);
            else Debug.Log("[HeldDelivery] You do not have a delivery to start the day");
            var package = interactingPlayer.PickupObject;
            interactingPlayer.DropPickup();
            package.transform.SetParent(packageContainer);
            package.transform.localPosition = Vector3.zero;
            
            if (_collectedDeliveries.Count < requiredDeliveries)
            {
                Debug.Log("You do not have enough deliveries to start the day");
                return;
            }
            var switchSceneAction = new SwitchSceneAction(sceneName);
            switchSceneAction.PerformAction(interactingPlayer, interactingObject);
        }

        public bool ConditionMet(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (_collectedDeliveries.Count >= requiredDeliveries) return true;
            return false;
        }
    }
}
