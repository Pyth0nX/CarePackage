using System;
using CarePackage.Delivery;
using UnityEngine;
using CarePackage.Main;
using TMPro;

namespace CarePackage.Interaction.Delivery
{
    [Serializable]
    public class PackageAction : InteractAction
    {
        [SerializeField] private SO_Package package;
        [SerializeField] private bool addedDelivery;

        public PackageAction() { package = null; }

        public PackageAction(Package inPackage)
        {
            package = DeliveryUitilities.ToScriptableObject(inPackage);
        }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            interactingPlayer.Pickup(interactingObject);
            if (addedDelivery) return;
            interactingPlayer.DeliveryManager.AddDelivery(DeliveryUitilities.ToPackage(package));
            addedDelivery = true;
        }
    }
    
    [Serializable]
    public class ReceiveDeliveryAction : InteractAction
    {
        [SerializeField] private int wantedPackage;
        
        private DeliveryManager _deliveryManager;
        
        public int WantedLetter { get => wantedPackage; set => wantedPackage = value; }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (interactingPlayer.DeliveryManager == null) return;
            _deliveryManager = interactingPlayer.DeliveryManager;
            
            ReceivePackage();
        }

        public bool ReceivePackage()
        {
            var packageToDeliver = _deliveryManager.GetCurrentDelivery();
            if (packageToDeliver == null) return false;
            
            if (packageToDeliver.Id != wantedPackage) return false;
            
            _deliveryManager.DeliverPackage(packageToDeliver);
            return true;
        }
    }
    
    [Serializable]
    public class SetListedJobAction : InteractAction, IActivatable
    {
        [SerializeField] private SO_Package job;
        [SerializeField] private GameObject parent;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            var jobManager = interactingPlayer.DeliveryManager;
            if (jobManager == null) return;

            jobManager.SetListedDelivery(DeliveryUitilities.ToPackage(job));
        }

        public void OnEnable()
        {
            var text = parent.GetComponentInChildren<TextMeshProUGUI>();
            text.text = job.PackageData.Title;
        }

        public void OnDisable()
        {
            Debug.Log($"[IActivatable:{this.GetType()}] OnDisable");
        }
    }
}