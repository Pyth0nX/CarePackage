using System;
using CarePackage.Delivery;
using CarePackage.Main;
using UnityEngine;

namespace CarePackage.Interaction
{
    [Serializable]
    public class MailAction : InteractAction
    {
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            Debug.Log($"{interactingPlayer} interacted with {interactingObject}");
        }
    }

    [Serializable]
    public class DeliverMail : InteractAction
    {
        [SerializeField] private int wantedPackage;

        public int id;
        
        private DeliveryManager _deliveryManager;
        
        public int WantedLetter { get => wantedPackage; set => wantedPackage = value; }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (interactingPlayer.DeliveryManager == null) return;
            _deliveryManager = interactingPlayer.DeliveryManager;
            
            DeliverMailToMailbox();
        }

        public bool DeliverMailToMailbox()
        {
            var packageToDeliver = _deliveryManager.GetCurrentDelivery();
            if (packageToDeliver == null) return false;
            
            if (packageToDeliver.Id != wantedPackage) return false;
            
            _deliveryManager.DeliverPackage(packageToDeliver);
            return true;
        }
    }
}