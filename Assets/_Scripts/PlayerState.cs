using CarePackage.Interaction;
using CarePackage.Job;
using UnityEngine;

namespace CarePackage.Main
{
    public class PlayerState : MonoBehaviour
    {
        // Components
        private JobManager _jobManager;
        public JobManager JobManager => _jobManager;
        private InteractionComponent _interactionComponent;
        public InteractionComponent InteractionComponent => _interactionComponent;

        private void Awake()
        {
            _jobManager = GetComponent<JobManager>();
            _interactionComponent = GetComponent<InteractionComponent>();
        }
    }
}
