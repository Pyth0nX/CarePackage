using CarePackage.Main;
using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;

namespace CarePackage.Interaction
{
    public class InteractionComponent : MonoBehaviour
    {
        [SerializeField] private GameObject interactionUI;
        [SerializeField] private LayerMask interactionLayer;
        [SerializeField, Range(0.1f, 50f)] private float rayDistance = 3.5f;
        [SerializeField] private bool castRay;
        [SerializeField] private bool debug;
        
        private TextMeshProUGUI _interactionText;
        private IInteractable _interactable;
        private float _elapsedTime;
        
        public bool ValidInteraction() => _interactable != null;

        private void Start()
        {
            if (interactionUI == null) return;
            _interactionText =  interactionUI.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (castRay) CheckForInteractions();
        }

        public void SetInteractable(IInteractable interactable)
        {
            if (interactable == null && _interactable != null)
                _interactable.OnHovered(false);

            _interactable = interactable;
/*
            if (_interactable == null && interactionUI != null) 
                interactionUI.SetActive(false);
            else if (_interactable.ShowMessage)
            {
                _interactionText.text = _interactable.InteractMessage;
                if (interactionUI != null) interactionUI.SetActive(true);
                _interactable.OnHovered(true);
            }*/
        }

        private void CheckForInteractions()
        {
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime < 0.2f) return;

            _elapsedTime = 0;

            var ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));

            if (Physics.Raycast(ray, out var hit, rayDistance, interactionLayer))
            {
                if (debug) Debug.Log($"[Interaction] raycast hit {hit.transform.name}");
                if (hit.collider.TryGetComponent(out IInteractable rayInteractable))
                {
                    if (debug) Debug.Log($"[Interaction] raycast got {rayInteractable.GetType().Name}");
                    if (rayInteractable.Type != EInteractionType.Active) return;
                    if (_interactable == rayInteractable) return;
                    
                    SetInteractable(rayInteractable);
                    return;
                }
            }
            SetInteractable(null);
        }

        public void OnInteract(InputAction.CallbackContext input)
        {
            if (!input.performed) return;

            var player = GameManager.Instance.Player;
            if (player == null) return;

            if (player.IsPickupValid)
            {/*
                if (_interactable != null)
                {
                    if (_interactable == player.PickupObject.GetComponent<IInteractable>())
                    {
                        player.DropPickup();
                        return;
                    }

                    if (((MonoBehaviour)_interactable).gameObject == player.PickupObject)
                    {
                        player.DropPickup();
                        return;
                    }
                }*/
                
                player.DropPickup();
                return;
            }
            
            /*
            if (!ValidInteraction())
            {
                if (player.IsPickupValid)
                {
                    player.DropPickup();
                }

                return;
            }*/

            if (ValidInteraction()) TryInteract();
        }

        public void TryInteract()
        {
            if (debug) Debug.Log("Trying to interaction with " + _interactable);
            if (_interactable == null) return;
            _interactable.ActivationType.RaiseInteraction();
            _interactable = null;
            if (interactionUI != null) interactionUI.SetActive(false);
        }

        private void OnDrawGizmos()
        {
            if (!debug) return;
            
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);
        }
    }
}