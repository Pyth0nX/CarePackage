using SerializeReferenceEditor;
using UnityEngine;

namespace CarePackage.Job
{
    [CreateAssetMenu(fileName = "Job", menuName = "CarePackage/Jobs/Job")]
    public class SO_Job : ScriptableObject
    {
        [SerializeField] private FJobData jobData;

        public FJobData JobData
        {
            get => jobData;
            set => jobData = value;
        }

        [SerializeReference, SR] private IJob job;
        public IJob Job => job;
    }
}