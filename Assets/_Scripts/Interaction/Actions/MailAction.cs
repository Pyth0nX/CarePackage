using System;
using CarePackage.Delivery;
using CarePackage.Main;
using UnityEngine;
using Random = UnityEngine.Random;

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
        [SerializeField] private SO_Job wantedLetter;
        
        private DeliveryManager _deliveryManager;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (interactingPlayer.DeliveryManager == null) return;
            _deliveryManager = interactingPlayer.DeliveryManager;
            
            DeliverMailToMailbox();
        }

        public bool DeliverMailToMailbox()
        {
            if (_deliveryManager.GetCurrentJob() != wantedLetter) return false;
            // _deliveryManager.DeliverPackage(wantedLetter);
            return true;
        }
    }
}