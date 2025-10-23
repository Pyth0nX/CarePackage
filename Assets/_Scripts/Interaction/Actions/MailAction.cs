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
        [SerializeField] private DeliveryManager deliveryManager;
        
        [SerializeField] private bool delivered;
        private int _lettersToDeliver = Random.Range(2, 4);
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            DeliverMoreMail(_lettersToDeliver);
        }
            public void DeliverMoreMail(int lettersToDeliver)
            {
                if (!delivered)
                {
                    Debug.Log("You have delivered mail");
                    _lettersToDeliver--;
                    delivered = true;
                }
                else
                {
                    Debug.Log("You have already delivered mail");
                }

                if (_lettersToDeliver == 0)
                {
                    Debug.Log("You have delivered all mail, congrats");
                }
            }
    }
}