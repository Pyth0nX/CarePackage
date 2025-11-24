using CarePackage.Interaction.Delivery;
using CarePackage.Interaction.Dialogue;
using System.Collections.Generic;
using CarePackage.Persistance;
using CarePackage.Interaction;
using CarePackage.Main;
using System.Linq;
using UnityEngine;
using Xasu.HighLevel;

namespace CarePackage.Delivery
{
    public class DeliveryManager : MonoBehaviour, IDataPersistance
    {
        [SerializeField] private Package currentDelivery;
        [SerializeField] private List<SO_Package>  deliveries = new();
        [SerializeField] private GameObject mailboxes;
        
        [SerializeField] private bool overrideRandomDelivery = false;
        [SerializeField] private int mainPackageNumber = 3;
        [SerializeField] private float newDeliveryDistanceThreshold;
        [SerializeField] private int minPackageAmount;
        [SerializeField] private int maxPackageAmount;

        [SerializeField] private int deliveriesToMake;

        [SerializeField] private SO_PredeterminedAddresses addressLibrary;
        [SerializeField] private SO_FlavourOptions flavourLibrary;
        [SerializeField] private SO_NonStoryItems itemLibrary;
        
        public int GetDeliveryQuotas => deliveriesToMake + _jobBoard.GetScriptedJobsByDayCount(GameManager.Instance.CurrentDay);
        public List<Package> Deliveries => DeliveryUitilities.ToPackageList(deliveries);
        public Package[] RequiredDeliveries => _jobBoard.GetScriptedJobsDeliveriesByDay(GameManager.Instance.CurrentDay);
        public DeliveryCheckList CheckList => _deliveryCheckList;
        public Package CurrentDelivery => _heldDelivery;
        public int CurrentDeliveryId => _currentDeliveryId;
        public int DeliveriesToMake => deliveries.Count;
        public int PackageMax { get => maxPackageAmount; set => maxPackageAmount = value; }

        private List<int> _randomNumbers = new();
        private StopWatch _deliveryTimer = new();
        private DeliveryCheckList _deliveryCheckList;
        private Package _mainDelivery;
        private Package _heldDelivery;
        private JobBoard _jobBoard;
        private int _deliveriesMade;
        private int _currentDeliveryId;
        private float _timeTakenToDelivery;
        private float _directDistanceToDelivery;
        
        private void Awake()
        {
            if (_jobBoard == null) _jobBoard = FindFirstObjectByType<JobBoard>();
            if (_deliveryCheckList == null) _deliveryCheckList = GetComponent<DeliveryCheckList>();
            if (mailboxes == null) mailboxes = GameObject.Find("Mailboxes");
        }

        private void Start()
        {
            LoadPackageMax();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                GetRandomJob();
            }
        }

        private void OnEnable()
        {
            Invoke("Enable", .001f);
        }

        private void Enable()
        {
            GameManager.Instance.onStartGame += OnDayStarted_Implementation;
        }

        private void OnDisable()
        {
            GameManager.Instance.onStartGame -= OnDayStarted_Implementation;
        }

        private void OnDayStarted_Implementation()
        {
            _deliveriesMade = 0;
            deliveries.Clear();
            MakeRandomNumbers(Random.Range(minPackageAmount, maxPackageAmount));
            Invoke("AssignRandomPackages", .1f);
        }
        
        private void EndDayEarly()
        {
            GameManager.Instance.EndDayEarly();
        }

        public void SetCurrentHeldDelivery(Package package)
        {
            _heldDelivery = package;
        }

        public void GetRandomJob()
        {
            var delivery = GetRandomAvailableJob();
            if (overrideRandomDelivery && _deliveriesMade == mainPackageNumber && _mainDelivery != null) 
                delivery = _mainDelivery;
            if (GameManager.Instance.ShouldAutomaticallyEndDayEarlyIfNoPackagesLeft && delivery == null)
            {
                GoalIndicator.Instance.SetGoalObject(null);
                Invoke("EndDayEarly", 2f);
                return;
            }
            SetNewJob(delivery);
            _deliveryCheckList.SelectPackage(currentDelivery);
        }

        public void SetNewJob(Package newJobPackage)
        {
            if (newJobPackage == null) return;
            var newJob = newJobPackage;
            
            int wantedId = newJob.Id;
            _currentDeliveryId = wantedId;
            Debug.Log("Wanted ID: " + wantedId);
            ToggleIndicator(wantedId, newJob);
            Debug.Log(newJob);
            _deliveryTimer.Start();
        }
        
        private Package GetRandomAvailableJob()
        {
            if (deliveries.Count == 0) return null;
            var randomIndex = Random.Range(0, deliveries.Count);
            return DeliveryUitilities.ToPackage(deliveries[randomIndex]);
        }

        public void AddDelivery(Package newDelivery, bool special = false)
        {
            deliveries.Add(DeliveryUitilities.ToScriptableObject(newDelivery));
            if (special) _mainDelivery =  newDelivery;
            _deliveryCheckList.InitializePackageList(Deliveries);
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
        
        public void SetListedDelivery(SetListedJobAction inJob, bool isLeft = false)
        {
            if (inJob == null) return;
            _jobBoard.SetJobListing(inJob, isLeft);
        }
        
        public void DeliverPackage(Package packageToDeliver)
        {
            var deliveringId = packageToDeliver.Id;
            var packageToDeliverSO = DeliveryUitilities.FindById(deliveries, deliveringId);
            if (packageToDeliverSO != null)
            {
                var packageObj = FindDeliveryPackageWithId(deliveringId);
                
                deliveries.Remove(packageToDeliverSO);
                _randomNumbers.Remove(deliveringId);
                
                _timeTakenToDelivery = _deliveryTimer.Stop();
                EconomyManager.Instance.CalculateMoneyEarned(packageToDeliver.PackageData, _timeTakenToDelivery, _directDistanceToDelivery);
                CompletableTracker.Instance.Completed("Package_" + packageToDeliver.PackageData.Title, CompletableTracker.CompletableType.Quest, _timeTakenToDelivery).WithSuccess(true);
                
                if (packageObj != null) Destroy(packageObj);
                _deliveryCheckList.CheckOffCurrentPackage();
                _deliveriesMade++;
                GetRandomJob();
                
                var distanceGoalToPlayer = Vector3.Distance(GoalIndicator.Instance.GoalTransform.position, 
                    GameManager.Instance.Player.ActivePlayer.transform.position);
                Debug.Log("Goal Distance: " + distanceGoalToPlayer);
                if (distanceGoalToPlayer > newDeliveryDistanceThreshold) return;
                
                var goalPackage = FindDeliveryPackageWithId(_currentDeliveryId);
                if (goalPackage == null) return;
                
                ToggleIndicator(goalPackage);
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
            var packages = new List<Package>();
            HelperMethods.ShuffleList(_randomNumbers);
            for (int i = 0; i < _randomNumbers.Count; i++)
            {
                var item = itemLibrary.GetRandomItem();
                int minPay = Random.Range(10, 100);
                int maxPay = Random.Range(minPay, minPay + 50);
                string flavour = flavourLibrary.GetRandomFlavour();
                string title = item.ItemData.name + "Delivery" + _randomNumbers[i];
                string address = "Go to Address: " + addressLibrary.GetAddressForId(i);
                string description =
                    $"Deliver to: {item.ItemData.name} wanted by {flavour}\n" +
                    "\n" +
                    $"Address: {address}\n" +
                    "\n" +
                    $"Pay: {minPay}-{maxPay}\n";
                
                var newDelivery = new Package
                {
                    Id = _randomNumbers[i],
                    PackageData = new FPackageData
                    (
                        title, 
                        description,
                        minPay,
                        maxPay
                        ),
                    ItemGUID = item != null ? item.ItemData.name : null,
                };
                //AddDelivery(newMail);
                //_jobBoard.CreateJob(newDelivery);
                packages.Add(newDelivery);
            }
            _jobBoard.CheckScriptedJobs();
            _jobBoard.InitRandomJobsForPackages(packages);
            _deliveryCheckList.InitializePackageList(packages);
        }

        public List<DeliverableZone> interactables = new();

        public void AssignRandomAddressesForDelivery()
        {
            var mailboxesCount = _randomNumbers.Count; 
            Debug.Log($"Assigning mailboxes random numbers: {mailboxesCount}");
            
            interactables = new();
            for (int i = 0; i < mailboxes.transform.childCount; i++)
            {
                var mailbox = mailboxes.transform.GetChild(i).GetComponentInChildren<DeliverableZone>();
                interactables.Add(mailbox);
            }
            HelperMethods.ShuffleList(_randomNumbers);
            
            for (int i = 0; i < mailboxesCount; i++)
            {
                int assignedNumber = _randomNumbers[i];
                Debug.Log($"Child {interactables[i].name} assigned number: {assignedNumber}");
                
                var action = interactables[i].InteractLogic;
                if (action is ReceiveDeliveryAction deliveryAction)
                {
                    deliveryAction.WantedPackage = assignedNumber;
                }
            }
            GetRandomJob();
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
            GameObject deliveryLocation = FindPostBoxWithId(wantedId);
            if (deliveryLocation == null) return;
            
            delivery.PackageData.MinPay = GetBasePayBasedOnDistance(transform.position, deliveryLocation.transform.position);
            SetCurrentDelivery(delivery);
            
            _directDistanceToDelivery = Vector3.Distance(transform.position, deliveryLocation.transform.position);
            ToggleIndicator(deliveryLocation);
        }

        public void ToggleIndicator(GameObject wantedGameObject, bool hasMapIndicator = true, bool hidePreviousMarker = true, float upOffset = 1.33f)
        {
            GoalIndicator.Instance.SetGoalObject(wantedGameObject, hasMapIndicator, hidePreviousMarker, upOffset);
        }

        public GameObject FindPostBoxWithId(int targetId)
        {
            GameObject foundPostBox = null;
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

                if (id != targetId || id == -1) continue;
                foundPostBox = postBox.gameObject;
                Debug.Log("Delivered ID: " + id + " Found Delivery: " + foundPostBox);
            }
            return foundPostBox;
        }

        public GameObject FindDeliveryPackageWithId(int targetId)
        {
            var foundPackage = deliveries.Find(d => d.Id == targetId);
            var packages = FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
            foreach (var package in packages)
            {
                var action = package.InteractAction;
                if (action is PackageAction packageAction)
                {
                    if (packageAction.Package.Id == targetId) return package.gameObject;
                }
            }
            return null;
        }

        private int GetBasePayBasedOnDistance(Vector3 position, Vector3 target)
        {
            var distance = Vector3.Distance(position, target);
            float normalizedDistance = Mathf.Clamp01((distance - 10f) / (400f - 10f));
            float baseValue = Mathf.Lerp(10f, 150f, normalizedDistance);
            return (int)baseValue;
        }

        public void RemovePackageFromConveyerBelt(GameObject package)
        {
            _jobBoard.RemovePackageFromConveyerBelt(package);
        }

        public void SavePackageMax(int max)
        {
            PlayerPrefs.SetInt("PackageMax", max);
            PackageMax = max;
        }
        
        public void LoadPackageMax()
        {
            maxPackageAmount = PlayerPrefs.GetInt("PackageMax", 4);
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