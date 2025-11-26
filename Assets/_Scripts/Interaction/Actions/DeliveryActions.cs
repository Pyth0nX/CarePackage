using CarePackage.Main;
using CarePackage.Delivery;
using UnityEngine;
using System;
using System.Linq;
using TMPro;

namespace CarePackage.Interaction.Delivery
{
    [Serializable]
    public class PackageAction : Pickup, IInteractAction, IActivatable
    {
        [SerializeField] private SO_Package package;
        [SerializeField] private bool addedDelivery;
        
        public Package Package => _internalPackage;
        public bool AlreadyAdded => addedDelivery;
        
        private PackageBehavior _packageBehavior;
        private Package _internalPackage;
        
        public void Add() => addedDelivery = true;
        
        public PackageAction(Package inPackage)
            : this(inPackage, false, new Vector3(0, -0.1f, 0), null) {}
        
        public PackageAction(Package inPackage, bool alreadyAdded)
            : this(inPackage, alreadyAdded, new Vector3(0, -0.1f, 0), null) {}
        
        public PackageAction(Package inPackage, bool alreadyAdded, Vector3 inOffset, GameObject inPickupOwningObject, IPickupExtension inAdditionalPickupExtension)
            : this(inPackage, alreadyAdded, inOffset, inPickupOwningObject, new[] {inAdditionalPickupExtension}) {}
        
        public PackageAction(Package inPackage, bool alreadyAdded, Vector3 inOffset, GameObject inPickupOwningObject, IPickupExtension[] inAddtioanlPickupExtensions = null) : base(inOffset, inPickupOwningObject)
        {
            _internalPackage = inPackage;
            package = DeliveryUitilities.ToScriptableObject(inPackage);
            addedDelivery = alreadyAdded;

            var packageObj = inPickupOwningObject.GetComponent<PackageBehavior>();
            _packageBehavior = packageObj;
            _packageBehavior.OnStateChanged += OnStateChanged_Implementation;
            var packagePickupExtension = new PackagePickupExtension(packageObj);
            //var damagePickup = new DamagableFieldExtension(new Vector3(0f, 0.6f, 1.15f), packageObj);

            int additionalExtensions = inAddtioanlPickupExtensions?.Length ?? 0;
            ExtendedLogic = new IPickupExtension[1 + additionalExtensions];
            ExtendedLogic[0] = packagePickupExtension;
            //ExtendedLogic[1] = damagePickup;
            
            if (additionalExtensions > 0)
                Array.Copy(inAddtioanlPickupExtensions, 0, ExtendedLogic, 1, additionalExtensions);
        }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (_internalPackage == null) _internalPackage = DeliveryUitilities.ToPackage(package);
            if (package == null) package = DeliveryUitilities.ToScriptableObject(_internalPackage);
            
            interactingPlayer.Pickup(this, interactingObject);
            Xasu.HighLevel.GameObjectTracker.Instance.Interacted("Package_" + Package.PackageData.Title, Xasu.HighLevel.GameObjectTracker.TrackedGameObject.Item);
            interactingPlayer.DeliveryManager.SetCurrentHeldDelivery(Package);
        }
        
        public void OnEnable()
        {
            if (_packageBehavior == null) return;
            if (!DelegateHelper.IsSubscribed(_packageBehavior.GetStateChangedDelegates(), (Action<EPackageState, EPackageState>)OnStateChanged_Implementation))
            {
                _packageBehavior.OnStateChanged += OnStateChanged_Implementation;
            }
        }

        public void OnDisable()
        {
            if (_packageBehavior == null) return;
            if (DelegateHelper.IsSubscribed(_packageBehavior.GetStateChangedDelegates(), (Action<EPackageState, EPackageState>)OnStateChanged_Implementation))
            {
                _packageBehavior.OnStateChanged -= OnStateChanged_Implementation;
            }
        }
        
        public static class DelegateHelper
        {
            public static bool IsSubscribed<T>(Delegate[] handlers, T method) where T : Delegate//public static bool IsSubscribed(Delegate[] handlers, Delegate target)
            {
                if (handlers == null || method == null)
                    return false;

                return handlers.Any(listener => listener == method);
            }
        }
        
        private void OnStateChanged_Implementation(EPackageState oldState, EPackageState newState)
        {
            _internalPackage.PackageData.State = newState;
        }
    }

    [Serializable]
    public class PackagePickupExtension : IPickupExtension
    {
        private readonly PackageBehavior _packageObj;

        public PackagePickupExtension(PackageBehavior inPackageObj)
        {
            _packageObj = inPackageObj;
        }
        
        public void ExtendedPickUp(PlayerState interactingPlayer)
        {
            _packageObj.VelocityThreshold = _packageObj.HeldVelocityThreshold;
            _packageObj.TogglePhysics(true);
            _packageObj.SetDamageEnabled(true);
        }

        public void ExtendedDropped(PlayerState interactingPlayer)
        {
            _packageObj.VelocityThreshold = _packageObj.DefaultVelocityThreshold;
            _packageObj.TogglePhysics(true);
            _packageObj.SetDamageEnabled(true);
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
            interactingPlayer.DeliveryManager.ToggleIndicator(_goalObject, true, false);//interactingPlayer.DeliveryManager.ToggleIndicator(_packageId, true, 0);
        }

        public void ExtendedDropped(PlayerState interactingPlayer)
        {
            if (!_overridePackageId) _packageId = interactingPlayer.DeliveryManager.CurrentDeliveryId;
            _goalObject = interactingPlayer.DeliveryManager.FindDeliveryPackageWithId(_packageId);
            interactingPlayer.DeliveryManager.ToggleIndicator(_goalObject, false, true);//interactingPlayer.DeliveryManager.ToggleIndicator(_packageId, true, 1);
        }
    }

    [Serializable]
    public class ReceiveDeliveryAction : IInteractAction
    {
        [SerializeField] private int wantedPackage;
        
        private DeliveryManager _deliveryManager;
        
        public int WantedPackage { get => wantedPackage; set => wantedPackage = value; }
        
        public ReceiveDeliveryAction() : this(-1) {}

        public ReceiveDeliveryAction(int inWantedPackage)
        {
            wantedPackage = inWantedPackage;
        }
        
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (interactingPlayer.DeliveryManager == null) return;
            _deliveryManager = interactingPlayer.DeliveryManager;
            
            var delivery = DeliveryUitilities.TryGetPackageFromObject(interactingObject);
            if (delivery == null)
            {
                Debug.Log("[ReceiveDeliveryAction] Could not find delivery");
                return;
            }
            
            Debug.Log("[ReceiveDeliveryAction] PerformAction " + delivery.Id);
            if (!CanReceivePackage2(delivery.Id)) return;
            interactingPlayer.DropPickup();
            _deliveryManager.DeliverPackage(delivery);
        }

        private bool CanReceivePackage()
        {
            if (_deliveryManager.CurrentDelivery.Id == wantedPackage) return true;
            return false;
        }
        
        private bool CanReceivePackage2(int incomingPackageId)
        {
            if (incomingPackageId != wantedPackage) return false;
            return true;
        }
    }
    
    [Serializable]
    public class ZoneReceivePackage : IInteractAction
    {
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            var packageAction = DeliveryUitilities.TryGetActionFromObject<PackageAction>(interactingObject);
            if (packageAction == null)
            {
                Debug.Log("[ZoneReceivePackage] Could not find package action");
                return;
            }

            if (packageAction.AlreadyAdded) return;
            packageAction.Add();
            
            var package = packageAction.Package;
            if (package == null)
            {
                Debug.Log("[ZoneReceivePackage] Could not find package action so no Package");
                return;
            }
            
            interactingPlayer.DeliveryManager.AddDelivery(package);
        }
    }
    
    [Serializable]
    public class SetListedJobAction : IInteractAction, IActivatable
    {
        [SerializeField] private SO_Package job;
        [SerializeField] private GameObject parent;
        [SerializeField] private bool isLeft;

        public Package Job => _internalPackage;
        public GameObject OwningObject => parent;
        
        private Package _internalPackage;
        
        public SetListedJobAction() : this(null, new Package(), false) {}
        
        public SetListedJobAction(GameObject inParent, Package inPackage) : this(inParent, inPackage, false) {}

        public SetListedJobAction(GameObject inParent, Package inPackage, bool inIsLeft)
        {
            parent = inParent;
            job = DeliveryUitilities.ToScriptableObject(inPackage);
            _internalPackage = inPackage;
            isLeft = inIsLeft;
        }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            var jobManager = interactingPlayer.DeliveryManager;
            if (jobManager == null) return;
            if (job == null || _internalPackage == null) return;
            
            Debug.Log("Clicked: " + parent.name);
            jobManager.SetListedDelivery(this, isLeft);
        }

        public void OnEnable()
        {
            var text = parent.GetComponentInChildren<TextMeshProUGUI>();
            text.text = _internalPackage.PackageData.Title;
            parent.name = _internalPackage.PackageData.Title;
        }

        public void OnDisable() {}
    }

    [Serializable]
    public class SelectDeliveryAction : IInteractAction
    {
        private Package _selectablePackage;
        private DeliveryCheckList _deliveryCheckList;
        private bool _deliveryCheckListInitialized;

        public SelectDeliveryAction() : this(null, null) {}
        
        public SelectDeliveryAction(DeliveryCheckList inDeliveryCheckList, Package inSelectablePackage)
        {
            _deliveryCheckList = inDeliveryCheckList;
            if (_deliveryCheckList != null) _deliveryCheckListInitialized = true;
            _selectablePackage = inSelectablePackage;
        }
        
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (!_deliveryCheckListInitialized) _deliveryCheckList = interactingPlayer.DeliveryManager.CheckList;
            if (_deliveryCheckList == null) return;
            _deliveryCheckList.SelectPackage(_selectablePackage);
            interactingPlayer.DeliveryManager.SetNewJob(_selectablePackage);
        }
    }
}