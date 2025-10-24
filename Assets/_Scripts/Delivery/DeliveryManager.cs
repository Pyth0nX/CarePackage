using System.Collections.Generic;
using CarePackage.Interaction;
using CarePackage.Interaction.Dialogue;
using CarePackage.Persistance;
using CarePackage.Main;
using System.Linq;
using UnityEngine;

namespace CarePackage.Delivery
{
    public class DeliveryManager : MonoBehaviour, IDataPersistance
    {
        [SerializeField] private IDeliverable currentDelivery;
        [SerializeField] private FPackageData debuggedJobData;
        [SerializeField] private List<IDeliverable>  deliveries = new();
        
        [SerializeField] private SO_Mail mailBase;
        [SerializeField] private GameObject mailboxes;
        
        [SerializeField] private bool overrideRandomDelivery = false;
        [SerializeField] private int mainPackageNumber = 3;

        [SerializeField] private int deliveriesToMake;
        
        private List<int> _randomNumbers = new();
        private int _deliveriesMade;
        private JobBoard _jobBoard;
        private IDeliverable _mainDelivery;

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
            GameManager.Instance.OnDayStarted += OnDayStarted_Implementation;
        }

        private void OnDisable()
        {
            GameManager.Instance.OnDayStarted -= OnDayStarted_Implementation;
        }

        private void OnDayStarted_Implementation()
        {
            _deliveriesMade = 0;
            MakeRandomNumbers(Random.Range(5, mailboxes != null ? mailboxes.transform.childCount : 30));
            Invoke("AssignMail", .1f);
        }

        private void GetNewJob()
        {
             IDeliverable delivery = GetRandomJob();
            if (overrideRandomDelivery && _deliveriesMade == mainPackageNumber && _mainDelivery != null) 
                delivery = _mainDelivery;
            int wantedId = 0;
            if (delivery is SO_Mail mail)
                wantedId = mail.AddressToDeliver;
            Debug.Log("Wanted ID: " + wantedId);
            ToggleIndicator(wantedId, delivery);
            _deliveryTimer.Start();
        }

        private void ToggleIndicator(int wantedId, IDeliverable delivery)
        {
            GameObject deliveryLocation = null;
            var postBoxes = FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
            
            foreach (var postBox in postBoxes)
            {
                var action = postBox.InteractAction;
                int id = -1;
                
                if (action is DeliverMail mailAction)
                    id =  mailAction.id;
                else if (action is DialogueAction dialogueAction)
                    id =  dialogueAction.id;
                else continue;

                if (id != wantedId || id == -1) continue;
                deliveryLocation = postBox.gameObject;
                Debug.Log("Delivered ID: " + id + " Found Delivery: " + deliveryLocation);
            }
            
            delivery.Pay = GetBasePayBasedOnDistance(transform.position, deliveryLocation.transform.position);
            SetCurrentDelivery(delivery);
            
            _directDistanceToDelivery = Vector3.Distance(transform.position, deliveryLocation.transform.position);
            STUPIDUITEST.Instance.SetObject(deliveryLocation);
        }

        private int GetBasePayBasedOnDistance(Vector3 position, Vector3 target)
        {
            var distance = Vector3.Distance(position, target);
            float normalizedDistance = Mathf.Clamp01((distance - 10f) / (1000f - 10f));
            float baseValue = Mathf.Lerp(50f, 500f, normalizedDistance);
            return (int)baseValue;
        }
        
        private IDeliverable GetRandomJob()
        {
            if (deliveries.Count == 0) return null;
            var randomIndex = Random.Range(0, deliveries.Count);
            return deliveries[randomIndex];
        }

        public void AddDelivery(IDeliverable newDelivery)
        {
            deliveries.Add(newDelivery);
            if (newDelivery is SO_Package newPackage)
            {
                _mainDelivery =  newPackage;
            }
        }

        public void SetCurrentDelivery(IDeliverable inDelivery)
        {
            Debug.Log($"[SetCurrrentJob] setting current job with so_job: {inDelivery}");
            currentDelivery = inDelivery;
        }
        
        public IDeliverable GetCurrentDelivery()
        {
            if (currentDelivery == null) return null;
            return currentDelivery;
        }
        
        public void SetListedDelivery(IDeliverable inJob)
        {
            SO_Package debugJob = (SO_Package)inJob;
            if (debugJob == null) return;
            debuggedJobData = debugJob.PackageData;
            _jobBoard.SetJobListing(debugJob);
        }
        
        public void DeliverPackage(IDeliverable packageToDeliver)
        {
            if (deliveries.Contains(packageToDeliver))
            {
                deliveries.Remove(packageToDeliver);
                _randomNumbers.Remove(packageToDeliver.Id);
                _timeTakenToDelivery = _deliveryTimer.Stop();
                EconomyManager.Instance.CalculateMoneyEarned(packageToDeliver.Pay, _timeTakenToDelivery, _directDistanceToDelivery);
                _deliveriesMade++;
                GetNewJob();
            }
        }

        private void AssignMail()
        {
            HelperMethods.ShuffleList(_randomNumbers);
            for (int i = 0; i < _randomNumbers.Count; i++)
            {
                var newMail = Instantiate(mailBase);
                newMail.AddressToDeliver = _randomNumbers[i];
                AddDelivery(newMail);
            }
            GetNewJob();
        }

        public void AssignMailBoxesRandom()
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
                if (action is DeliverMail mailAction)
                {
                    mailAction.WantedLetter = assignedNumber;
                    mailAction.id = assignedNumber;
                }
            }
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

        public void LoadData(GameData loadData)
        {
            deliveries = loadData.deliveries.ToList();
            SetCurrentDelivery(loadData.currentDelivery);
        }

        public void SaveData(GameData saveData)
        {
            saveData.deliveries = deliveries.ToArray();
            saveData.currentDelivery = GetCurrentDelivery();
        }
    }
}