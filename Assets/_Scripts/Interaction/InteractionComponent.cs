using CarePackage.Main;
using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;

namespace CarePackage.Interaction
{
    public class InteractionComponent : MonoBehaviour
    {
        [SerializeField] private LayerMask interactionLayer;
        [SerializeField] private bool debug;
        [SerializeField] private bool castRay;
        [SerializeField, Range(0.1f, 50f)] private float rayDistance = 4.5f;
        [SerializeField] private bool isPlayer = true;
        [SerializeField] private GameObject interactionUI;
        
        private IInteractable _interactable;
        private MonoBehaviour owner;
        
        private TextMeshProUGUI _interactionText;
        private float _elapsedTime;
        
        public bool ValidInteraction() => _interactable != null;
        public bool IsPassive => _interactable.Type == InteractionType.Passive;
        public bool IsActive => _interactable.Type == InteractionType.Active;

        private void Start()
        {
            if (isPlayer) owner = GameManager.Instance.Player;
            else owner = transform.root.GetComponent<MonoBehaviour>();
            _interactionText =  interactionUI.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (castRay) CheckForInteractions();
        }

        public void SetInteractable(IInteractable interactable)
        {
            _interactable = interactable;
            
            if (_interactable == null) interactionUI.SetActive(false);
            else
            {
                _interactionText.text = _interactable.InteractText;
                interactionUI.SetActive(true);
            }
        }

        private void CheckForInteractions()
        {
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime < 0.2f) return;

            _elapsedTime = 0;

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f)); // new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayDistance, interactionLayer))
            {
                if (debug) Debug.Log($"[Interaction] raycast hit {hit.transform.name}");
                if (hit.collider.gameObject.TryGetComponent(out IInteractable rayInteractable))
                {
                    if (debug) Debug.Log($"[Interaction] raycast got {rayInteractable.GetType().Name}");

                    if (_interactable == rayInteractable) return;
                    SetInteractable(rayInteractable);
                    return;
                }
            }
            SetInteractable(null);
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
        
        public void OnInteract(InputAction.CallbackContext input)
        {
            if (input.started)
            {
                if (!ValidInteraction() || !IsActive) return;
                TryInteract();
                interactionUI.SetActive(false);
            }
        }

        public void TryInteract()
        {
            if (debug) Debug.Log("Trying to interaction with " + _interactable);
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