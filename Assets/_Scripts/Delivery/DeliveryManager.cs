using System.Collections.Generic;
using System.Linq;
using CarePackage.Persistance;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CarePackage.Delivery
{
    public class DeliveryManager : MonoBehaviour, IDataPersistance
    {
        [SerializeField] private SO_Job currentDelivery;
        [SerializeField] private FJobData debuggedJobData;
        [SerializeField] private List<SO_Job> deliveries = new();
        
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
            SetCurrentJob(GetRandomJob());
        }

        public void CompleteJob(SO_Job deliveryToComplete)
        {
            if (deliveries.Contains(deliveryToComplete))
            {
                deliveries.Remove(deliveryToComplete);
                GetNewJob();
            }
        }

        private SO_Job GetRandomJob()
        {
            if (deliveries.Count == 0) return null;
            var randomIndex = Random.Range(0, deliveries.Count);
            return deliveries[randomIndex];
        }

        public void SetCurrentJob(SO_Job inJob)
        {
            Debug.Log($"[SetCurrrentJob] setting current job with so_job: {inJob}");
            currentDelivery = inJob;
        }
        
        public SO_Job GetCurrentJob()
        {
            if (currentDelivery == null) return null;
            return currentDelivery;
        }

        public void SetListedJob(SO_Job inJob)
        {
            debuggedJobData = inJob.JobData;
            _jobBoard.SetJobListing(inJob);
        }

        public void LoadData(GameData loadData)
        {
            deliveries = loadData.deliveries.ToList();
            SetCurrentJob(loadData.currentDelivery);
        }

        public void SaveData(GameData saveData)
        {
            saveData.deliveries = deliveries.ToArray();
            saveData.currentDelivery = GetCurrentJob();
        }
    }
}