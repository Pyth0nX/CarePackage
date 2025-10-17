using System;
using CarePackage.Job;
using UnityEngine;
using CarePackage.Main;
using SerializeReferenceEditor;
using TMPro;

namespace CarePackage.Interaction
{
    [Serializable]
    public class PackageAction : InteractAction
    {
        [SerializeField] private SO_Job job;
        
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            interactingPlayer.JobManager.SetCurrentJob(job);
            interactingPlayer.Pickup(interactingObject);
        }
    }
    
    [Serializable]
    public class SetJob : InteractAction
    {
        [SerializeReference, SR] private IJob job;
        
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            var JobManager = interactingPlayer.JobManager;
            if (JobManager == null) return;
            
            JobManager.SetCurrrentJob(job);
        }
    }

    [Serializable]
    public class SetJobFromBoard : InteractAction, IActivatable
    {
        [SerializeField] private SO_Job job;
        [SerializeField] private GameObject parent;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            var jobManager = interactingPlayer.JobManager;
            if (jobManager == null) return;

            jobManager.SetCurrrentJob(job.Job);
        }
        
        public void OnEnable()
        {
            Debug.Log($"[IActivatable:{this.GetType()}] OnEnable");
            var text = parent.GetComponentInChildren<TextMeshProUGUI>();
            text.text = job.JobData.Title;
        }

        public void OnDisable()
        {
            Debug.Log($"[IActivatable:{this.GetType()}] OnDisable");
        }
    }
}