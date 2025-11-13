using CarePackage.Interaction;
using UnityEngine;

namespace CarePackage.Task
{
    public class TaskBehavior : MonoBehaviour
    {
        [SerializeField] private SO_Task task;
        [SerializeField] private bool popOnAwake;
        [SerializeField] private bool popOnInteracted;
        [SerializeField] private bool popOnCompleted;
        [SerializeField] private Interactable interactable;

        private void Awake()
        {
            if (popOnAwake)
            {
                PopTask();
            }
        }

        private void OnEnable()
        {
            if (interactable == null) return;
            if (popOnCompleted) interactable.OnInteractionFinished += PopTask;
            else if (popOnInteracted) interactable.OnInteracted += PopTask;
        }

        private void OnDisable()
        {
            if (interactable == null) return;
            if (popOnCompleted) interactable.OnInteractionFinished -= PopTask;
            else if (popOnInteracted) interactable.OnInteracted -= PopTask;
        }

        public void PushTask()
        {
            TaskManager.PushTaskUpdate(task.Task);
        }

        public void PopTask()
        {
            
        }
    }
}