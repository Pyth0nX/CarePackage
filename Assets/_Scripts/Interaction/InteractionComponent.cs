using CarePackage.Main;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CarePackage.Interaction
{
    public class InteractionComponent : MonoBehaviour
    {
        [SerializeField] private LayerMask interactionLayer;
        [SerializeField] private bool debug;
        [SerializeField] private bool castRay;
        [SerializeField, Range(0.1f, 10f)] private float rayDistance = 4.5f;
        
        private IInteractable _interactable;
        private MonoBehaviour owner;

        private float _elapsedTime;
        
        public bool ValidInteraction() => _interactable != null;
        public bool IsPassive => _interactable.Type == InteractionType.Passive;
        public bool IsActive => _interactable.Type == InteractionType.Active;

        private void Start()
        {
            owner = transform.root.GetComponent<MonoBehaviour>();
        }

        private void Update()
        {
            if (castRay) CheckForInteractions();
        }

        public void SetInteractable(IInteractable interactable)
        {
            _interactable = interactable;
        }

        private void CheckForInteractions()
        {
            if (_elapsedTime >= .2f)
            {
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));// new Ray(transform.position, transform.forward);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, rayDistance, interactionLayer))
                {
                    Debug.Log($"[Interaction] raycast hit {hit.transform.name}");
                    if (hit.collider.gameObject.TryGetComponent(out IInteractable rayInteractable))
                    {
                        Debug.Log($"[Interaction] raycast got {rayInteractable.GetType().Name}");
                        SetInteractable(rayInteractable);
                    }
                }
                _elapsedTime = 0;
            }
            _elapsedTime += Time.deltaTime;
        }

        /*
        private void OnTriggerEnter(Collider other)
        {/*
            if ((interactionLayer.value & (1 << gameObject.layer)) == 0)
            {
                Debug.LogWarning($"[Interactable] Layer mismatch: {gameObject.name} is on layer {gameObject.layer}, not in {interactionLayer}");
                return;
            }

            if (other.gameObject.TryGetComponent<IInteractable>(out _interactable))
            {
                if (IsPassive)
                {
                    Debug.LogWarning($"[Interactable] Passive interaction: {gameObject.name}");
                    TryInteract(owner as PlayerState);
                }
            }
        }*/
/*
        private void OnTriggerExit(Collider other)
        {
            if ((interactionLayer.value & (1 << gameObject.layer)) == 0)
            {
                Debug.LogWarning($"[Interactable] Layer mismatch: {gameObject.name} is on layer {gameObject.layer}, not in {interactionLayer}");
                return;
            }
            _interactable = null;
        }*/
        
        public void Interact(InputAction.CallbackContext input)
        {
            if (input.started)
            {
                if (!ValidInteraction() || !IsActive) return;
                TryInteract();
            }
        }

        public void TryInteract()
        {
            Debug.Log("Trying to interaction with " + _interactable);
            if (_interactable == null) return;
            _interactable.Interact(owner as PlayerState);
            _interactable = null;
        }

        private void OnDrawGizmos()
        {
            if (!debug) return;
            
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);
        }
    }
}