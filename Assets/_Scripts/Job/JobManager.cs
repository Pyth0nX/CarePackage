using SerializeReferenceEditor;
using UnityEngine;

namespace CarePackage.Job
{
    public class JobManager : MonoBehaviour
    {
        [SerializeReference, SR] private IJob _job;
        [SerializeField] private JobBoard jobBoard;
        
        public void SetCurrrentJob(IJob job)
        {
            if (job == null) return;
            _job = job;
            jobBoard.SetJobListing(_job);
        }
    }
}