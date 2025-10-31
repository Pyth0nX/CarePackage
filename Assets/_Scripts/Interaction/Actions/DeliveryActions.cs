using System;
using CarePackage.Delivery;
using UnityEngine;
using CarePackage.Main;
using TMPro;

namespace CarePackage.Interaction.Delivery
{
    [Serializable]
    public class PackageAction : InteractAction, IPickup
    {
        [SerializeField] private SO_Package package;
        [SerializeField] private bool addedDelivery;
        [SerializeField] private Vector3 offset;

        public Package Package => DeliveryUitilities.ToPackage(package);
        
        private PackageObject _packageObj;
        private GameObject _owningObject;

        public PackageAction() { package = null; }

        public PackageAction(Package inPackage)
        {
            package = DeliveryUitilities.ToScriptableObject(inPackage);
        }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            interactingPlayer.Pickup(this, interactingObject);
            if (addedDelivery) return;
            interactingPlayer.DeliveryManager.AddDelivery(DeliveryUitilities.ToPackage(package));
            addedDelivery = true;
        }

        public Vector3 Offset => offset;
        public GameObject OwningObject { get => _owningObject; set => _owningObject = value; }
        
        public void OnPickedUp(PlayerState interactingPlayer)
        { 
            _packageObj = OwningObject.GetComponent<PackageObject>();
            _packageObj.TogglePhysics(false);
            _packageObj.VelocityThreshold = _packageObj.HeldVelocityThreshold;
        }

        public void OnDropped(PlayerState interactingPlayer)
        {
            _packageObj.TogglePhysics(true);
            _packageObj.VelocityThreshold = _packageObj.DefaultVelocityThreshold;
        }
    }
    
    [Serializable]
    public class ReceiveDeliveryAction : InteractAction
    {
        [SerializeField] private int wantedPackage;
        
        private DeliveryManager _deliveryManager;
        
        public int WantedPackage { get => wantedPackage; set => wantedPackage = value; }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (interactingPlayer.DeliveryManager == null) return;
            _deliveryManager = interactingPlayer.DeliveryManager;
            
            if (!CanReceivePackage()) return;
            ReceivePackage();
        }

        private bool CanReceivePackage()
        {
            if (_deliveryManager.CurrentDelivery.Id == wantedPackage) return true;
            return false;
        }

        public void ReceivePackage()
        {
            var packageToDeliver = _deliveryManager.GetCurrentDelivery();
            if (packageToDeliver == null) return;
            
            if (packageToDeliver.Id != wantedPackage) return;
            
            _deliveryManager.DeliverPackage(packageToDeliver);
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