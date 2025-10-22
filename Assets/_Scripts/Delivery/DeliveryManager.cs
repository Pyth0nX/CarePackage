using UnityEngine;

namespace CarePackage.Delivery
{
    public class DeliveryManager : MonoBehaviour
    {
        [SerializeField] private SO_Job job;
        [SerializeField] private FJobData debuggedJobData;
        
        private JobBoard _jobBoard;

        private int[] unreadMails;
        
        private void Start()
        {
            _jobBoard = FindFirstObjectByType<JobBoard>();
        }

        public void SetCurrentJob(SO_Job inJob)
        {
            Debug.Log($"[SetCurrrentJob] setting current job with so_job: {inJob}");
            if (job == null) return;
            job = inJob;
            debuggedJobData = job.JobData;
            _jobBoard.SetJobListing(job);
        }

        public SO_Job GetCurrentJob()
        {
            if (job == null) return null;
            return job;
        }
    }
}