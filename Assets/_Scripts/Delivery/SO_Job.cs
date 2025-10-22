using UnityEngine;

namespace CarePackage.Delivery
{
    [CreateAssetMenu(fileName = "Job", menuName = "CarePackage/Jobs/Job")]
    public class SO_Job : ScriptableObject
    {
        [SerializeField] private FJobData jobData;
        [SerializeField] private SO_Item item;

        public FJobData JobData => jobData;
    }
}