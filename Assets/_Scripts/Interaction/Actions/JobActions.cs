using System;
using CarePackage.Job;
using UnityEngine;
using CarePackage.Main;
using SerializeReferenceEditor;
using TMPro;
using UnityEngine.Scripting.APIUpdating;

namespace CarePackage.Interaction.Package
{
    [MovedFrom("CarePackage.Interaction")]
    [Serializable]
    public class PackageAction : InteractAction
    {
        [SerializeField] private SO_Job job;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            interactingPlayer.Pickup(interactingObject);
            interactingPlayer.JobManager.SetCurrentJob(job);
        }
    }

    [MovedFrom("CarePackage.Interaction")]
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

    [MovedFrom("CarePackage.Interaction")]
    [Serializable]
    public class SetJobFromBoard : InteractAction, IActivatable
    {
        [SerializeField] private SO_Job job;
        [SerializeField] private GameObject parent;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            var jobManager = interactingPlayer.JobManager;
            if (jobManager == null) return;

            jobManager.SetCurrentJob(job);
        }

        public void OnEnable()
        {
            var text = parent.GetComponentInChildren<TextMeshProUGUI>();
            text.text = job.JobData.Title;
        }

        public void OnDisable()
        {
            Debug.Log($"[IActivatable:{this.GetType()}] OnDisable");
        }
    }
}