using SerializeReferenceEditor;
using UnityEngine;

namespace CarePackage.Job
{
    public class JobManager : MonoBehaviour
    {
        [SerializeReference, SR] private IJob _job;
        [SerializeField] private SO_Job _jobDetails;
        [SerializeField] private JobBoard jobBoard;
        
        public void SetCurrrentJob(IJob job)
        {
            if (job == null) return;
            _job = job;
            jobBoard.SetJobListing(_job);
        }

        public void SetCurrentJob(SO_Job job)
        {
            if (job == null) return;
            _jobDetails = job;
            SetCurrrentJob(_jobDetails.Job);
        }

        public SO_Job GetCurrentJob()
        {
            if (_jobDetails == null) return null;
            return _jobDetails;
        }
    }
}