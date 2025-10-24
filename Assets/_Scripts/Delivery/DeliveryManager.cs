using System.Collections.Generic;
using System.Linq;
using CarePackage.Interaction;
using CarePackage.Interaction.Dialogue;
using CarePackage.Main;
using CarePackage.Persistance;
using UnityEngine;
using TMPro;

namespace CarePackage.Delivery
{
    public class DeliveryManager : MonoBehaviour, IDataPersistance
    {
        [SerializeField] private IDeliverable currentDelivery;
        [SerializeField] private FPackageData debuggedJobData;
        [SerializeField] private List<IDeliverable>  deliveries = new();
        
        [SerializeField] private SO_Mail mailBase;
        [SerializeField] private GameObject mailboxes;
        [SerializeField] private TextMeshProUGUI mailLocationText;
        
        private List<int> _randomNumbers = new();
        private JobBoard _jobBoard;

        private void Awake()
        {
            if (_jobBoard == null) _jobBoard = FindFirstObjectByType<JobBoard>(FindObjectsInactive.Include);
            if (mailboxes == null) mailboxes = GameObject.Find("Mailboxes");
        }

        private void Start()
        {
            
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
            GameManager.Instance.OnDayStarted += OnDayStarted;
        }

        private void OnDisable()
        {
            GameManager.Instance.OnDayStarted -= OnDayStarted;
        }

        private void OnDayStarted()
        {
            AssignMail(new int[Random.Range(8, 16)].ToList());
        }

        private void GetNewJob()
        {
            var delivery = GetRandomJob();
            SetCurrentDelivery(delivery);

            int wantedId = Random.Range(0, 30);
            Debug.Log("Wanted ID: " + wantedId);
            GameObject deliveryLocation = null;

            var postBoxes = FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
            foreach (var postBox in postBoxes)
            {
                var action = postBox.InteractAction;
                int id = -1;
                if (action is DeliverMail mailAction)
                {
                    id =  mailAction.id;
                }
                else if (action is DialogueAction dialogueAction)
                {
                    id =  dialogueAction.id;
                }
                else
                {
                    continue;
                }

                if (id != wantedId || id == -1)
                {
                    Debug.Log("ID: " + id + " is not the wanted ID: " + wantedId);
                    continue;
                }
                deliveryLocation = postBox.gameObject;
                Debug.Log("Set DeliveryLocation to: " + deliveryLocation);
            }
            
            if (deliveryLocation == null) return;
            {
                Debug.Log("Delivery Location is null: " + deliveryLocation);
            }
            var indicator = STUPIDUITEST.Instance;
            Debug.Log("Indicator = " + indicator);
            indicator.SetObject(deliveryLocation);
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
                GetNewJob();
            }
        }

        private void AssignMail(List<int> randomNumbers)
        {
            HelperMethods.ShuffleList(randomNumbers);
            for (int i = 0; i < randomNumbers.Count; i++)
            {
                var newMail = Instantiate(mailBase);
                newMail.AddressToDeliver = randomNumbers[i];
                AddDelivery(newMail);
            }
            GetNewJob();
        }

        public void AssignMailBoxesRandom()
        {
            var mailboxesCount = mailboxes.transform.childCount; 
            Debug.Log($"Assigning mailboxes random numbers: {mailboxesCount}");
            
            List<Interactable> interactables = new List<Interactable>();
            for (int i = 0; i < mailboxesCount; i++)
            {
                var mailbox = mailboxes.transform.GetChild(i).GetComponent<Interactable>();
                interactables.Add(mailbox);
            }
            MakeRandomNumbersWithShuffle(mailboxesCount);
            
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
        }
        
        public void MakeRandomNumbersWithShuffle(int number)
        { 
            MakeRandomNumbers(number);
            HelperMethods.ShuffleList(_randomNumbers);
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