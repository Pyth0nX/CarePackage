using CarePackage.Main;
using CarePackage.Delivery;
using UnityEngine;
using System;
using TMPro;

namespace CarePackage.Interaction.Delivery
{
    [Serializable]
    public class PackageAction : Pickup, IInteractAction
    {
        [SerializeField] private SO_Package package;
        [SerializeField] private bool addedDelivery;

        public Package Package => _internalPackage;
        
        private Package _internalPackage;
        
        public PackageAction(Package inPackage, bool alreadyAdded, Vector3 inOffset, GameObject inPickupOwningObject, IPickupExtension[] inAddtioanlPickupExtensions = null) : base(inOffset, inPickupOwningObject)
        {
            _internalPackage = inPackage;
            package = DeliveryUitilities.ToScriptableObject(inPackage);
            addedDelivery = alreadyAdded;

            var packageObj = inPickupOwningObject.GetComponent<PackageObject>();
            var packagePickupExtension = new PackagePickupExtension(packageObj);
            
            if (inAddtioanlPickupExtensions == null || inAddtioanlPickupExtensions.Length == 0) ExtendedLogic = new IPickupExtension[] { packagePickupExtension };
            else
            {
                ExtendedLogic = new IPickupExtension[inAddtioanlPickupExtensions.Length + 1];
                ExtendedLogic[0] = packagePickupExtension;
                Array.Copy(inAddtioanlPickupExtensions, 0, ExtendedLogic, 1, inAddtioanlPickupExtensions.Length);
            }
        }
        
        public PackageAction(Package inPackage, bool alreadyAdded, Vector3 inOffset, GameObject inPickupOwningObject, IPickupExtension inAdditionalPickupExtension) 
            : this(inPackage, alreadyAdded, inOffset, inPickupOwningObject, new[] {inAdditionalPickupExtension}) {}
        
        public PackageAction(Package inPackage, bool alreadyAdded) 
            : this(inPackage, alreadyAdded, new Vector3(0, -0.1f, 0), null) {}
        
        public PackageAction(Package inPackage) 
            : this(inPackage, false, new Vector3(0, -0.1f, 0), null) {}

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (_internalPackage == null) _internalPackage = DeliveryUitilities.ToPackage(package);
            if (package == null) package = DeliveryUitilities.ToScriptableObject(_internalPackage);
            
            interactingPlayer.Pickup(this, interactingObject);
            
            if (addedDelivery) return;
            interactingPlayer.DeliveryManager.AddDelivery(DeliveryUitilities.ToPackage(package));
            addedDelivery = true;
        }
    }

    [SerializeField]
    public class PackagePickupExtension : IPickupExtension
    {
        private readonly PackageObject _packageObj;

        public PackagePickupExtension(PackageObject inPackageObj)
        {
            _packageObj = inPackageObj;
        }
        
        public void ExtendedPickUp(PlayerState interactingPlayer)
        {
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
    public class ConveyerBeltPackageExtension : IPickupExtension
    {
        private readonly GameObject _packageObject;
        
        public ConveyerBeltPackageExtension(GameObject inPackageObject)
        {
            _packageObject = inPackageObject;
        }
        
        public void ExtendedPickUp(PlayerState interactingPlayer)
        {
            interactingPlayer.DeliveryManager.RemovePackageFromConveyerBelt(_packageObject);
        }

        public void ExtendedDropped(PlayerState interactingPlayer) {}
    }
    
    [Serializable]
    public class IndicatorPickupDroppableExtension : IPickupExtension
    {
        private GameObject _goalObject;
        private int _packageId;
        private bool _overridePackageId;

        public IndicatorPickupDroppableExtension(int inPackageId = -1)
        {
            _goalObject = null;
            _overridePackageId = false;
            if (inPackageId == -1) return;
            _packageId = inPackageId;
            _overridePackageId = true;
        }
        
        public void ExtendedPickUp(PlayerState interactingPlayer)
        {
            if (!_overridePackageId) _packageId = interactingPlayer.DeliveryManager.CurrentDeliveryId;
            _goalObject = interactingPlayer.DeliveryManager.FindPostBoxWithId(_packageId);
            interactingPlayer.DeliveryManager.ToggleIndicator(_goalObject, true, false);
        }

        public void ExtendedDropped(PlayerState interactingPlayer)
        {
            if (!_overridePackageId) _packageId = interactingPlayer.DeliveryManager.CurrentDeliveryId;
            _goalObject = interactingPlayer.DeliveryManager.FindDeliveryPackageWithId(_packageId);
            interactingPlayer.DeliveryManager.ToggleIndicator(_goalObject, false, false, 0);
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