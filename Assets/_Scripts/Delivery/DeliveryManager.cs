using System.Collections.Generic;
using System.Linq;
using CarePackage.Persistance;
using UnityEngine;

namespace CarePackage.Delivery
{
    public class DeliveryManager : MonoBehaviour, IDataPersistance
    {
        [SerializeField] private IDeliverable currentDelivery;
        [SerializeField] private FPackageData debuggedJobData;
        [SerializeField] private List<IDeliverable>  deliveries = new();
        
        private JobBoard _jobBoard;
        
        private void Start()
        {
            if (_jobBoard == null) _jobBoard = FindFirstObjectByType<JobBoard>(FindObjectsInactive.Include);
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
            
        }

        private void OnDisable()
        {
            
        }

        private void GetNewJob()
        {
            SetCurrentDelivery(GetRandomJob());
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