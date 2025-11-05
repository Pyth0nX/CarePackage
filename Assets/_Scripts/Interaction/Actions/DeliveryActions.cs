using CarePackage.Main;
using CarePackage.Delivery;
using UnityEngine;
using System;
using TMPro;

namespace CarePackage.Interaction.Delivery
{
    [Serializable]
    public class PackageAction : IInteractAction, IPickupExtension
    {
        [SerializeField] private SO_Package package;
        [SerializeField] private bool addedDelivery;
        [SerializeField] private Vector3 offset;

        public Package Package => _internalPackage;
        public Vector3 Offset => offset;
        
        private PackageObject _packageObj;
        private Package _internalPackage;
        private Miscellaneous.PickupAction _pickupAction;

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

        public void PickupAction(Miscellaneous.PickupAction pickupAction = null)
        {
            if (pickupAction == null) _pickupAction = new Miscellaneous.PickupAction();
            _pickupAction = pickupAction;
        }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (_internalPackage == null) _internalPackage = DeliveryUitilities.ToPackage(package);
            if (package == null) package = DeliveryUitilities.ToScriptableObject(_internalPackage);
            
            if (_pickupAction != null)
                _pickupAction.PerformAction(interactingPlayer, interactingObject);
            
            if (addedDelivery) return;
            interactingPlayer.DeliveryManager.AddDelivery(DeliveryUitilities.ToPackage(package));
            addedDelivery = true;
        }

        public void ExtendedPickUp(PlayerState interactingPlayer)
        {
            _packageObj = _pickupAction.OwningObject.GetComponent<PackageObject>();
            _packageObj.VelocityThreshold = _packageObj.HeldVelocityThreshold;
            _packageObj.TogglePhysics(false);
        }

        public void ExtendedDropped(PlayerState interactingPlayer)
        {
            _packageObj.TogglePhysics(true);
            _packageObj.VelocityThreshold = _packageObj.DefaultVelocityThreshold;
        }
    }

    [Serializable]
    public class ConveyerBeltPackagePickup : IPickupExtension
    {
        [SerializeField] private GameObject packageObject;
        
        public ConveyerBeltPackagePickup(GameObject inPackageObject)
        {
            packageObject = inPackageObject;
        }
        
        public void ExtendedPickUp(PlayerState interactingPlayer)
        {
            interactingPlayer.DeliveryManager.RemovePackageFromConveyerBelt(packageObject);
        }

        public void ExtendedDropped(PlayerState interactingPlayer) {}
    }
    
    [Serializable]
    public class NeigbourHoodPackagePickup : IPickupExtension
    {
        private GameObject _goalObject;
        private int _packageId;

        public NeigbourHoodPackagePickup(int inPackageId, GameObject inGoalObject)
        {
            _packageId = inPackageId;
            _goalObject = inGoalObject;
        }
        
        public void ExtendedPickUp(PlayerState interactingPlayer)
        {
            _goalObject = interactingPlayer.DeliveryManager.FindPostBoxWithId(_packageId);
            interactingPlayer.DeliveryManager.ToggleIndicator(_goalObject, true, false);
        }

        public void ExtendedDropped(PlayerState interactingPlayer)
        {
            _goalObject = interactingPlayer.DeliveryManager.FindDeliveryPackageWithId(_packageId);
            interactingPlayer.DeliveryManager.ToggleIndicator(_goalObject, false, false);
        }
    }

    [Serializable]
    public class ReceiveDeliveryAction : IInteractAction
    {
        [SerializeField] private int wantedPackage;
        
        private DeliveryManager _deliveryManager;
        
        public int WantedPackage { get => wantedPackage; set => wantedPackage = value; }

        public ReceiveDeliveryAction(int inWantedPackage)
        {
            wantedPackage = inWantedPackage;
        }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (interactingPlayer.DeliveryManager == null) return;
            _deliveryManager = interactingPlayer.DeliveryManager;
            
            if (!CanReceivePackage()) return;
            var delivery = _deliveryManager.CurrentDelivery;
            _deliveryManager.DeliverPackage(delivery);
        }

        private bool CanReceivePackage()
        {
            if (_deliveryManager.CurrentDelivery.Id == wantedPackage) return true;
            return false;
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