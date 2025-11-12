using System;
using UnityEngine;

namespace CarePackage.Task
{
    [CreateAssetMenu(fileName = "SO_Task", menuName = "CarePackage/Task")]
    public class SO_Task : ScriptableObject
    {
        [SerializeField] private Task task;
        
        public Task Task => task;
    }
    
    [Serializable]
    public class Task
    {
        public string description;
        
        public Task(string inDesription)
        {
            description = inDesription;
        }
    }
}
