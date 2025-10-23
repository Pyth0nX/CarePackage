using System;
using CarePackage.Delivery;
using UnityEngine;
using CarePackage.Main;
using TMPro;

namespace CarePackage.Interaction.Delivery
{
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
    
    [Serializable]
    public class SetListedJobAction : InteractAction, IActivatable
    {
        [SerializeField] private SO_Job job;
        [SerializeField] private GameObject parent;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            var jobManager = interactingPlayer.DeliveryManager;
            if (jobManager == null) return;

            jobManager.SetListedJob(job);
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