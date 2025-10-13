using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Job", menuName = "CarePackage/Jobs/Job")]
public class SO_Job : ScriptableObject
{
    [SerializeField] private FJobData jobData;
    public FJobData JobData { get => jobData; set => jobData = value; }
    
    [SerializeReference, SR] private IJob job;
}