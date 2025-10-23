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
        [SerializeField] private int wantedLetter;
        
        private DeliveryManager _deliveryManager;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (interactingPlayer.DeliveryManager == null) return;
            _deliveryManager = interactingPlayer.DeliveryManager;
            
            DeliverMailToMailbox();
        }

        public bool DeliverMailToMailbox()
        {
            var currentPackage = _deliveryManager.GetCurrentDelivery();
            if (currentPackage == null) return false;

            if (currentPackage is not SO_Mail mailPackage) return false;
            if (mailPackage.id != wantedLetter) return false;
            
            _deliveryManager.DeliverPackage(mailPackage);
            return true;
        }
    }
}