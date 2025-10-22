using System;
using CarePackage.Delivery;
using UnityEngine;
using CarePackage.Main;
using TMPro;
using UnityEngine.Scripting.APIUpdating;

namespace CarePackage.Interaction.Package
{
    [MovedFrom("CarePackage.Interaction")]
    [Serializable]
    public class PackageAction : InteractAction
    {
        [SerializeField] private SO_Job job;

        public PackageAction() { job = null; }

        public PackageAction(SO_Job inJob)
        {
            job = inJob;
        }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            interactingPlayer.Pickup(interactingObject);
            interactingPlayer.DeliveryManager.SetCurrentJob(job);
        }
    }

    [MovedFrom("CarePackage.Interaction")]
    [Serializable]
    public class SetJob : InteractAction
    {
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            var JobManager = interactingPlayer.DeliveryManager;
            if (JobManager == null) return;

            //JobManager.SetCurrrentJob(job);
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
            var jobManager = interactingPlayer.DeliveryManager;
            if (jobManager == null) return;

            
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