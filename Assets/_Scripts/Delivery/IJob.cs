
using UnityEngine;

namespace CarePackage.Delivery
{
    public interface IJob
    {
        public string GetTitle();
        public string GetDescription();
    }

    [System.Serializable]
    public struct FJobData
    {
        public string Title;
        public string Description;
    }

    [System.Serializable]
    public class JobBob : IJob
    {
        [SerializeField] private SO_Job job;
        private FJobData _data;
        public string bob;

        public string GetTitle()
        {
            return job.JobData.Title;
        }

        public string GetDescription()
        {
            return job.JobData.Description;
        }
    }
}