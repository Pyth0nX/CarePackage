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
        }

        private void OnDisable()
        {
            OnTaskUpdated -= OnTaskUpdated_Implementation;
        }

        private void OnTaskUpdated_Implementation(Task incomingTask)
        {
            Tween.StopAll(taskPanel.transform);
            _taskText.text = incomingTask.description;
            taskPanel.SetActive(true);
            Tween.Delay(taskPanel.transform, taskPanelDuration).OnComplete(() => taskPanel.SetActive(false));
        }
        
        public static void PopTaskUpdate(Task task)
        {
            OnTaskUpdated?.Invoke(task);
        }
    }
}