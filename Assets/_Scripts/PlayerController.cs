using CarePackage.Interaction;
using UnityEngine.InputSystem;
using UnityEngine;

namespace CarePackage.Main
{
    public class PlayerController : MonoBehaviour
    {
        // Editor variables
        [SerializeField] private float speed = 5f;
        
        [SerializeField] private float jumpForce = 5f;
        
        [SerializeField, Range(0.01f, 4f)] private float sensitivity = 1f;
        
        [SerializeField] private GameObject playerCamera;
        
        // private components
        private InteractionComponent _interactionComponent;
        private PlayerState _owningPlayer;
        private Rigidbody _rb;
        
        // private variables
        private Vector3 _movementVector;
        private Vector2 _inputVector;
        private Vector2 _lookVector;
        private Vector3 _velocity;
        private float _horizontalCameraSpeed;
        private float _verticalCameraSpeed;
        private float _xRotation;
        private float _yRotation;
        private float _sprintBonus = 1f;
        private bool _isGrounded;
        private bool _lockedInput;
        
        public bool IsGrounded => _isGrounded;
        public void LockInput(bool lockMode) => _lockedInput = lockMode;
        
        private void Start()
        {
            InitializeAllComponents();
        }
        
        private void InitializeAllComponents()
        {
            _rb = GetComponentInChildren<Rigidbody>();
            _owningPlayer = GetComponentInChildren<PlayerState>();
            _interactionComponent = GetComponentInChildren<InteractionComponent>();
        }

        private void OnEnable()
        {
            Invoke("Enable", .1f);
        }

        private void Enable()
        {
            Debug.Log($"[{GetType()}] Is UIManager Instance set {UIManager.Instance != null}");
            UIManager.Instance.OnInterfaceOpened += () => ListenToUIChanges(true);
            UIManager.Instance.OnInterfaceClosed += ListenToUIChanges;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable()
        {
            UIManager.Instance.OnInterfaceOpened -= () => ListenToUIChanges(true);
            UIManager.Instance.OnInterfaceClosed -= ListenToUIChanges;
            LockCursor(CursorLockMode.None);
        }

        private void ListenToUIChanges(bool uiChange)
        {
            LockInput(uiChange);
            LockCursor(uiChange ? CursorLockMode.None : CursorLockMode.Locked);
        }

        private void LockCursor(CursorLockMode lockMode)
        {
            Cursor.lockState = lockMode;
        }

        private void FixedUpdate()
        {
            if (_lockedInput) return;
            if (_rb == null) return;
            
            _isGrounded = Physics.Raycast(transform.position, -transform.up, out var ground, .65f);
            
            HandleLook();
            HandleVectors();
            
            _rb.linearVelocity = _velocity;
        }
        
        private void HandleVectors()
        {
            Vector3 input = speed * _sprintBonus * new Vector3(_inputVector.x, 0, _inputVector.y);
            _movementVector = transform.TransformDirection(input);
            
            _velocity = new Vector3(_movementVector.x, _rb.linearVelocity.y, _movementVector.z);
        }

        private void HandleLook()
        {
            _horizontalCameraSpeed = _lookVector.x * sensitivity;
            _verticalCameraSpeed = _lookVector.y * sensitivity;
            
            _yRotation = _horizontalCameraSpeed;
            
            _xRotation -= _verticalCameraSpeed;
            _xRotation = Mathf.Clamp(_xRotation, -45, 90);
            playerCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
            transform.Rotate(Vector3.up * _yRotation);
        }
        
        public void Look(InputAction.CallbackContext input)
        {
            if (input.performed) _lookVector = input.ReadValue<Vector2>();
            else if (input.canceled) _lookVector = Vector2.zero;
        }

        public void Move(InputAction.CallbackContext input)
        {
            if (input.performed) _inputVector = input.ReadValue<Vector2>();
            else if (input.canceled) _inputVector = Vector2.zero;
        }

        public void Run(InputAction.CallbackContext input)
        {
            if (input.started) _sprintBonus = 2f;
            else if (input.canceled) _sprintBonus = 1f;
        }

        public void Jump(InputAction.CallbackContext input)
        {
            if (input.started)
            {
                Vector3 force;
                if (_movementVector.magnitude > 0.1f)
                {
                    force = Vector3.up + Vector3.forward * .5f;
                }
                else
                {
                    force = Vector3.up;
                }
                _rb.AddForce(force.normalized * jumpForce, ForceMode.VelocityChange);
                _isGrounded = false;
                Debug.Log($"Player Jumped");
            }
        }
        
        private void OnDrawGizmos()
        {
            Debug.DrawRay(transform.position, -Vector3.up * .65f, Color.yellow);
        }
    }
}