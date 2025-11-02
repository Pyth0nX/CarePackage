using System.Collections.Generic;
using System.Linq;
using CarePackage.Interaction;
using CarePackage.Interaction.Delivery;
using CarePackage.Interaction.Dialogue;
using CarePackage.Persistance;
using CarePackage.Main;
using UnityEngine;

namespace CarePackage.Delivery
{
    public class DeliveryManager : MonoBehaviour, IDataPersistance
    {
        [SerializeField] private Package currentDelivery;
        [SerializeField] private List<SO_Package>  deliveries = new();
        [SerializeField] private GameObject mailboxes;
        
        [SerializeField] private bool overrideRandomDelivery = false;
        [SerializeField] private int mainPackageNumber = 3;

        [SerializeField] private int deliveriesToMake;
        
        public Package CurrentDelivery => _heldDelivery;
        
        private List<int> _randomNumbers = new();
        private int _deliveriesMade;
        private JobBoard _jobBoard;
        private Package _mainDelivery;
        private Package _heldDelivery;

        private StopWatch _deliveryTimer = new();
        private float _timeTakenToDelivery;
        private float _directDistanceToDelivery;
        
        private void Awake()
        {
            if (_jobBoard == null) _jobBoard = FindFirstObjectByType<JobBoard>(FindObjectsInactive.Include);
            if (mailboxes == null) mailboxes = GameObject.Find("Mailboxes");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                GetNewJob();
            }
        }

        private void OnEnable()
        {
            Invoke("Enable", .001f);
        }

        private void Enable()
        {
            Debug.Log("GameManager: " + GameManager.Instance);
            GameManager.Instance.OnStartGame += OnDayStarted_Implementation;
        }

        private void OnDisable()
        {
            GameManager.Instance.OnStartGame -= OnDayStarted_Implementation;
        }

        private void OnDayStarted_Implementation()
        {
            _deliveriesMade = 0;
            deliveries.Clear();
            MakeRandomNumbers(Random.Range(5, 12));
            Invoke("AssignRandomPackages", .1f);
        }
        
        private void EndDayEarly()
        {
            GameManager.Instance.EndDayEarly();
        }

        public void SetCurrentHeldDelivery(Package package)
        {
            if (package == null) return;
            _heldDelivery = package;
        }

        public void GetNewJob()
        {
            var delivery = GetRandomJob();
            if (overrideRandomDelivery && _deliveriesMade == mainPackageNumber && _mainDelivery != null) 
                delivery = _mainDelivery;
            if (delivery == null && GameManager.Instance.ShouldAutomaticallyEndDayEarlyIfNoPackagesLeft)
            {
                GoalIndicator.Instance.SetGoalObject(null);
                Invoke("EndDayEarly", 2f);
                return;
            }
            int wantedId = delivery.Id;
            Debug.Log("Wanted ID: " + wantedId);
            ToggleIndicator(wantedId, delivery);
            _deliveryTimer.Start();
        }
        
        private Package GetRandomJob()
        {
            if (deliveries.Count == 0) return null;
            var randomIndex = Random.Range(0, deliveries.Count);
            return DeliveryUitilities.ToPackage(deliveries[randomIndex]);
        }

        public void AddDelivery(Package newDelivery, bool special = false)
        {
            deliveries.Add(DeliveryUitilities.ToScriptableObject(newDelivery));
            if (special) _mainDelivery =  newDelivery;
        }

        public void SetCurrentDelivery(Package inDelivery)
        {
            Debug.Log($"[SetCurrrentJob] setting current job with so_job: {inDelivery}");
            currentDelivery = inDelivery;
        }
        
        public Package GetCurrentDelivery()
        {
            if (currentDelivery == null) return null;
            return currentDelivery;
        }
        
        public void SetListedDelivery(Package inJob)
        {
            if (inJob == null) return;
            _jobBoard.SetJobListing(inJob);
        }
        
        public void DeliverPackage(Package packageToDeliver)
        {
            var packageToDeliverSO = DeliveryUitilities.FindById(deliveries, packageToDeliver.Id);
            if (packageToDeliverSO != null)
            {
                deliveries.Remove(packageToDeliverSO);
                _randomNumbers.Remove(packageToDeliver.Id);
                _timeTakenToDelivery = _deliveryTimer.Stop();
                EconomyManager.Instance.CalculateMoneyEarned(packageToDeliver.PackageData.Pay, _timeTakenToDelivery, _directDistanceToDelivery);
                _deliveriesMade++;
                GetNewJob();
            }
        }
        
        private SO_Package GetDeliveryById(int id, SO_Package packageToDeliver)
        {
            var match = deliveries.FirstOrDefault(d => d.Id == packageToDeliver.Id);
            if (match != null) return match;
            return null;
        }

        private void AssignRandomPackages()
        {
            HelperMethods.ShuffleList(_randomNumbers);
            for (int i = 0; i < _randomNumbers.Count; i++)
            {
                string Title = "Delivery" + _randomNumbers[i];
                string Description = "Go to Address: " + _randomNumbers[i];
                var newDelivery = new Package()
                {
                    Id = _randomNumbers[i],
                    PackageData = new FPackageData(
                        Title, 
                        Description, 
                        Random.Range(10, 150)
                        )
                };
                //AddDelivery(newMail);
                _jobBoard.CreateJob(newDelivery);
            }
        }

        public void AssignRandomAddressesForDelivery()
        {
            var mailboxesCount = _randomNumbers.Count; 
            Debug.Log($"Assigning mailboxes random numbers: {mailboxesCount}");
            
            List<Interactable> interactables = new();
            for (int i = 0; i < mailboxes.transform.childCount; i++)
            {
                var mailbox = mailboxes.transform.GetChild(i).GetComponent<Interactable>();
                interactables.Add(mailbox);
            }
            HelperMethods.ShuffleList(_randomNumbers);
            
            for (int i = 0; i < mailboxesCount; i++)
            {
                int assignedNumber = _randomNumbers[i];
                Debug.Log($"Child {interactables[i].name} assigned number: {assignedNumber}");
                
                var action = interactables[i].InteractAction;
                if (action is ReceiveDeliveryAction deliveryAction)
                {
                    deliveryAction.WantedPackage = assignedNumber;
                }
            }
            GetNewJob();
        }

        public void MakeRandomNumbers(int number)
        {
            _randomNumbers.Clear();
            for (int i = 0; i < number; i++)
            {
                _randomNumbers.Add(i);
            }
            deliveriesToMake = number;
        }
        
        private void ToggleIndicator(int wantedId, Package delivery)
        {
            GameObject deliveryLocation = null;
            var postBoxes = FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
            
            foreach (var postBox in postBoxes)
            {
                var action = postBox.InteractAction;
                int id = -1;
                
                if (action is ReceiveDeliveryAction mailAction)
                    id =  mailAction.WantedPackage;
                else if (action is DialogueAction dialogueAction)
                    id =  dialogueAction.id;
                else continue;

                if (id != wantedId || id == -1) continue;
                deliveryLocation = postBox.gameObject;
                Debug.Log("Delivered ID: " + id + " Found Delivery: " + deliveryLocation);
            }
            
            delivery.PackageData.Pay = GetBasePayBasedOnDistance(transform.position, deliveryLocation.transform.position);
            SetCurrentDelivery(delivery);
            
            _directDistanceToDelivery = Vector3.Distance(transform.position, deliveryLocation.transform.position);
            GoalIndicator.Instance.SetGoalObject(deliveryLocation);
        }

        private int GetBasePayBasedOnDistance(Vector3 position, Vector3 target)
        {
            var distance = Vector3.Distance(position, target);
            float normalizedDistance = Mathf.Clamp01((distance - 10f) / (400f - 10f));
            float baseValue = Mathf.Lerp(10f, 150f, normalizedDistance);
            return (int)baseValue;
        }

        public void LoadData(GameData loadData)
        {
            if (loadData.deliveries != null) deliveries = DeliveryUitilities.ToScriptableObjectList(loadData.deliveries);
            SetCurrentDelivery(loadData.currentDelivery);
            _randomNumbers = loadData.randomNumbers;
        }

        public void SaveData(GameData saveData)
        {
            if (deliveries != null) saveData.deliveries = DeliveryUitilities.ToPackageList(deliveries).ToArray();
            saveData.currentDelivery = GetCurrentDelivery();
            if (_randomNumbers.Count > 0) saveData.randomNumbers = _randomNumbers;
        }
    }
}