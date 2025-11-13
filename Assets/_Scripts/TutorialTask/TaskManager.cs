using PrimeTween;
using UnityEngine;
using TMPro;

namespace CarePackage.Task
{
    public class TaskManager : MonoBehaviour
    {
        [SerializeField] private GameObject taskPanel;
        [SerializeField] private float taskPanelDuration;

        private TextMeshProUGUI _taskText;
        
        public static event System.Action<Task> OnTaskUpdated;
        public static event System.Action OnTaskCancelled;

        private void Start()
        {
            FetchComponents();
        }

        private void FetchComponents()
        {
            _taskText = taskPanel.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            OnTaskUpdated += OnTaskUpdated_Implementation;
            OnTaskCancelled += PopTaskInternal;
        }

        private void OnDisable()
        {
            OnTaskUpdated -= OnTaskUpdated_Implementation;
            OnTaskCancelled -= PopTaskInternal;
        }

        private void OnTaskUpdated_Implementation(Task incomingTask)
        {
            PopTask();
            _taskText.text = incomingTask.description;
            taskPanel.SetActive(true);
            Tween.Delay(taskPanel.transform, taskPanelDuration).OnComplete(() => taskPanel.SetActive(false));
        }
        
        private void PopTaskInternal()
        {
            Tween.StopAll(taskPanel.transform);
        }
        
        public static void PushTaskUpdate(Task task)
        {
            OnTaskUpdated?.Invoke(task);
        }

        public static void PopTask()
        {
            OnTaskCancelled?.Invoke();
        }
    }
}