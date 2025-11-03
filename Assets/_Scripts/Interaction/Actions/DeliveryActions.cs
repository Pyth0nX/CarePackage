using System;
using CarePackage.Delivery;
using UnityEngine;
using CarePackage.Main;
using TMPro;

namespace CarePackage.Interaction.Delivery
{
    [Serializable]
    public class PackageAction : IInteractAction, IPickup
    {
        [SerializeField] private SO_Package package;
        [SerializeField] private bool addedDelivery;
        [SerializeField] private Vector3 offset;

        public Package Package => _internalPackage;
        
        private PackageObject _packageObj;
        private GameObject _owningObject;
        private Package _internalPackage;

        public PackageAction() { package = null; }

        public PackageAction(Package inPackage)
        {
            _internalPackage = inPackage;
            package = DeliveryUitilities.ToScriptableObject(inPackage);
        }
        
        public PackageAction(Package inPackage, bool alreadyAdded) : this(inPackage)
        {
            addedDelivery = alreadyAdded;
        }
        
        public PackageAction(Package inPackage, bool alreadyAdded, Vector3 inOffset) : this(inPackage, alreadyAdded)
        {
            offset = inOffset;
        }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (_internalPackage == null) _internalPackage = DeliveryUitilities.ToPackage(package);
            if (package == null) package = DeliveryUitilities.ToScriptableObject(_internalPackage);
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
            ExtendedOnPickedUp(interactingPlayer);
        }

        protected virtual void ExtendedOnPickedUp(PlayerState interactingPlayer)
        {
            interactingPlayer.DeliveryManager.RemovePackageFromConveyerBelt(_packageObj.gameObject);
        }

        public void OnDropped(PlayerState interactingPlayer)
        {
            _packageObj.TogglePhysics(true);
            _packageObj.VelocityThreshold = _packageObj.DefaultVelocityThreshold;
        }
    }

    [Serializable]
    public class PackageInSceneAction : PackageAction
    {
        public PackageInSceneAction(Package inPackage) : base(inPackage) { }
        public PackageInSceneAction(Package inPackage, bool alreadyAdded) : base(inPackage, alreadyAdded) { }
        public PackageInSceneAction(Package inPackage, bool alreadyAdded, Vector3 inOffset) : base(inPackage, alreadyAdded, inOffset) { }
        protected override void ExtendedOnPickedUp(PlayerState interactingPlayer)
        {
            base.ExtendedOnPickedUp(interactingPlayer);
            var wantedId = interactingPlayer.DeliveryManager.CurrentDeliveryId;
            var postBox = interactingPlayer.DeliveryManager.FindPostBoxWithId(wantedId);
            interactingPlayer.DeliveryManager.ToggleIndicator(postBox);
        }
    }

    [Serializable]
    public class ReceiveDeliveryAction : IInteractAction
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
            var f = _deliveryManager.FindDeliveryPackageWithId(packageToDeliver.Id);
            if (packageToDeliver.Id != wantedPackage) return;
            
            _deliveryManager.DeliverPackage(packageToDeliver);
            GameObject.Destroy(f);
        }
    }
    
    [Serializable]
    public class SetListedJobAction : IInteractAction, IActivatable
    {
        [SerializeField] private SO_Package job;
        [SerializeField] private GameObject parent;
        private Package _internalPackage;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            var jobManager = interactingPlayer.DeliveryManager;
            if (jobManager == null) return;
            if (job == null) return;
            if (_internalPackage == null) _internalPackage = DeliveryUitilities.ToPackage(job);

            jobManager.SetListedDelivery(_internalPackage);
        }

        public void OnEnable()
        {
            var text = parent.GetComponentInChildren<TextMeshProUGUI>();
            text.text = _internalPackage.PackageData.Title;
        }

        public void OnDisable()
        {
            Debug.Log($"[IActivatable:{this.GetType()}] OnDisable");
        }
        
        public void SetParent(GameObject inParent) => parent = inParent;

        public void SetJob(Package inJob)
        {
            job = DeliveryUitilities.ToScriptableObject(inJob);
            _internalPackage = inJob;
        }
    }
}