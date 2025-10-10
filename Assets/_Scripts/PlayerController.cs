using CarePackage.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CarePackage.Main
{
    public class PlayerController : MonoBehaviour
    {
        // Editor variables
        [SerializeField]
        private float speed = 5f;
        
        // private components
        private PlayerState _owningPlayer;
        private Rigidbody _rb;
        private InteractionComponent _interactionComponent;
        
        // private variables
        private Vector2 _movementVector;
        private Vector2 _inputVector;
        private float _sprintBonus = 1f;
        
        private void Start()
        {
            InitializeAllComponents();
        }

        private void InitializeAllComponents()
        {
            _rb = GetComponentInChildren<Rigidbody>();
            _owningPlayer = GetComponent<PlayerState>();
            _interactionComponent = GetComponent<InteractionComponent>();
            
            //_animController = GetComponent<AnimationController>();
        }

        private void FixedUpdate()
        {
            if (_rb == null) return;
            _rb.linearVelocity = _movementVector * speed;
        }
        
        public void Move(InputAction.CallbackContext input)
        {
            _inputVector = input.ReadValue<Vector2>();
            UpdateMovementVector();
        }
        
        private void UpdateMovementVector()
        {
            _movementVector = _inputVector * _sprintBonus;
            //_animController.Input(_movementVector);
        }

        public void Run(InputAction.CallbackContext input)
        {
            if (input.started) _sprintBonus = 2f;
            else if (input.canceled) _sprintBonus = 1f;
            UpdateMovementVector();
        }

        public void Interact(InputAction.CallbackContext input)
        {
            if (input.started)
            {
                _interactionComponent.TryInteract(_owningPlayer);
            }
        }

        public void Jump(InputAction.CallbackContext input)
        {
            if (input.started)
            {
                Debug.Log($"Player Jumped");
            }
        }
    }
}