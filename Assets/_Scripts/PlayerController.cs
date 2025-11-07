using System;
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
        [SerializeField] private float groundCheckSize;
        [SerializeField] private Vector3 groundCheckOffset;
        [SerializeField] private bool useExtendedGroundRay;
        [SerializeField] private GameObject playerCamera;
        public float airControlRate = 2f;

        // private components
        private PlayerState _owningPlayer;
        private Rigidbody _rb;

        // private variables
        private RaycastSensor _groundRay;
        private Vector3 _movementVector;
        private Vector2 _inputVector;
        private Vector2 _lookVector;
        private Vector3 _velocity;
        private Vector3 _momentum;
        private float _horizontalCameraSpeed;
        private float _verticalCameraSpeed;
        private float _xRotation;
        private float _yRotation;
        private float _sprintBonus = 1f;
        [SerializeField] private bool _isGrounded;
        private bool _lockedInput;
        [SerializeField] private bool _isJumping;

        public bool IsGrounded => _isGrounded;
        public void LockInput(bool lockMode) => _lockedInput = lockMode;

        public static event Action OnPlayerMoved = delegate { };

        private void Start()
        {
            InitializeAllComponents();
        }

        private void LateUpdate()
        {
            _groundRay.DrawDebug();
        }

        private void InitializeAllComponents()
        {
            _rb = transform.root.GetComponent<Rigidbody>();
            _owningPlayer = GameManager.Instance.Player;
            var capCol = GetComponent<CapsuleCollider>();
            _groundRay ??= new RaycastSensor(transform);
            _groundRay.SetCastOrigin(capCol.bounds.center);
            _groundRay.SetCastDirection(RaycastSensor.CastDirection.Down);
            var length = 2 * (1f) * 0.5f + 2;
            var baseLength = length * 1f * transform.lossyScale.x;
            _groundRay.castLength = baseLength * transform.localScale.x;
            _groundRay.layermask = gameObject.layer;
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
            LockCursor(CursorLockMode.Locked);
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

        private void Update()
        {
            if (_lockedInput) return;

            Debug.Log($"Casting Ray with: {_groundRay.castLength} length, owner: {_groundRay._owningTransform}, offset: {_groundRay._origin}, direction: {_groundRay.castDirection} on layer: {_groundRay.layermask}");
            HandleLook();
            //HandleVectors();
        }

        private void FixedUpdate()
        {
            if (_lockedInput) return;
            if (_rb == null) return;

            //_isGrounded = Physics.Raycast(transform.position, -transform.up, out var ground, groundCheckSize);
            /*Vector3 origin = transform.position + Vector3.up * 0.1f; // Slight offset to avoid self-collision
            _isGrounded = Physics.SphereCast(origin, groundCheckSize, Vector3.down, out var groundHit, 2);*/
            /*Vector3 checkPosition = transform.position + groundCheckOffset;
            _isGrounded = Physics.CheckSphere(checkPosition, groundCheckSize);*
            //GroundCheck();
            //HandleMomentum();
            GroundCheck();
            HandleMomentum();
            Vector3 velocity = IsGrounded ? CalculateMovementDirection() * speed : Vector3.zero;
            velocity += _momentum;

            _rb.linearVelocity = velocity;*/
            
            ComputeFinalVelocity();
        }
        
        private void ComputeFinalVelocity()
        {
            // Convert input to world-space movement
            Vector3 input = new Vector3(_inputVector.x, 0, _inputVector.y);
            Vector3 moveDirection = transform.TransformDirection(input);
            moveDirection = moveDirection.magnitude > 1f ? moveDirection.normalized : moveDirection;

            // Horizontal movement
            Vector3 horizontalVelocity = moveDirection * speed * _sprintBonus;

            // Apply gravity to vertical momentum
            _momentum = transform.localToWorldMatrix * _momentum;
            Vector3 verticalMomentum = VectorMath.ExtractDotVector(_momentum, transform.up);
            Vector3 horizontalMomentum = _momentum - verticalMomentum;

            verticalMomentum -= transform.up * 30 * Time.fixedDeltaTime;

            if (IsGrounded && VectorMath.GetDotProduct(verticalMomentum, transform.up) < 0f)
                verticalMomentum = Vector3.zero;

            // Air control
            if (!IsGrounded)
                AdjustHorizontalMomentum(ref horizontalMomentum, horizontalVelocity);

            // Friction
            float friction = IsGrounded ? 100 : .5f;
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, friction * Time.fixedDeltaTime);

            // Apply jump
            if (_isJumping)
                verticalMomentum = transform.up * jumpForce;

            // Combine
            _momentum = horizontalMomentum + verticalMomentum;
            _momentum = transform.worldToLocalMatrix * _momentum;

            // Final velocity
            Vector3 finalVelocity = IsGrounded ? horizontalVelocity : Vector3.zero;
            finalVelocity += _momentum;

            _rb.linearVelocity = finalVelocity;
        }
        
        Vector3 CalculateMovementDirection() {
            Vector3 direction = transform.right * _inputVector.x + transform.forward * _inputVector.y; //== null 
                /*Vector3.ProjectOnPlane(transform.right, transform.up).normalized * _inputVector.x + 
                  Vector3.ProjectOnPlane(transform.forward, transform.up).normalized * _inputVector.y;*/
            
            return direction.magnitude > 1f ? direction.normalized : direction;
        }

        private void HandleMomentum()
        {
            _momentum = transform.localToWorldMatrix * _momentum;
            Vector3 verticalMomentum = VectorMath.ExtractDotVector(_momentum, transform.up);
            Vector3 horizontalMomentum = _momentum - verticalMomentum;
            
            verticalMomentum -= transform.up * (30f * Time.deltaTime);
            if (IsGrounded && VectorMath.GetDotProduct(verticalMomentum, transform.up) < 0f) {
                verticalMomentum = Vector3.zero;
            }
            
            if (!IsGrounded) {
                AdjustHorizontalMomentum(ref horizontalMomentum, _movementVector);
            }
            
            float friction = IsGrounded ? 100 : 0.5f;
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, friction * Time.deltaTime);
            
            _momentum = horizontalMomentum + verticalMomentum;
            
            if (_isJumping) {
                HandleJumping();
            }
            
            _momentum = transform.worldToLocalMatrix * _momentum;
        }

        private void AdjustHorizontalMomentum(ref Vector3 horizontalMomentum, Vector3 verticalMomentum)
        {
            if (horizontalMomentum.magnitude > speed) {
                if (VectorMath.GetDotProduct(_movementVector, horizontalMomentum.normalized) > 0f) {
                    _movementVector = VectorMath.RemoveDotVector(_movementVector, horizontalMomentum.normalized);
                }
                horizontalMomentum += _movementVector * (Time.deltaTime * airControlRate * 0.25f);
            }
            else {
                horizontalMomentum += _movementVector * (Time.deltaTime * airControlRate);
                horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, speed);
            }
        }

        private void HandleJumping()
        {
            _momentum = VectorMath.RemoveDotVector(_momentum, transform.up);
            _momentum += transform.up * jumpForce;
        }

        private void StartJump()
        {
            if (!IsGrounded || _isJumping) return;

            _momentum = transform.worldToLocalMatrix * _momentum;
            _isJumping = true;

            PrimeTween.Tween.Delay(0.2f).OnComplete(() => _isJumping = false);
            /*
            _momentum = transform.worldToLocalMatrix * _momentum;
            
            _momentum += transform.up * jumpForce;
            _isJumping = true;
            PrimeTween.Tween.Delay(.2f).OnComplete(() => _isJumping = false);
            
            _momentum = transform.worldToLocalMatrix * _momentum;*/
        }

        private void GroundCheck()
        {
            _groundRay.castLength = useExtendedGroundRay 
                ? groundCheckSize + 2 * transform.localScale.x
                : groundCheckSize;
            _groundRay.Cast();

            _isGrounded = _groundRay.HasDetectedHit();
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
            // transform.Rotate(Vector3.up * _yRotation);
            _rb.MoveRotation(_rb.rotation * Quaternion.AngleAxis(_yRotation, Vector3.up));
        }
        
        private void LockCursor(CursorLockMode lockMode)
        {
            Cursor.lockState = lockMode;
        }
        
        public void OnLook(InputAction.CallbackContext input)
        {
            if (input.performed) _lookVector = input.ReadValue<Vector2>();
            else if (input.canceled) _lookVector = Vector2.zero;
            OnPlayerMoved?.Invoke();
        }

        public void OnMove(InputAction.CallbackContext input)
        {
            if (input.performed) _inputVector = input.ReadValue<Vector2>();
            else if (input.canceled) _inputVector = Vector2.zero;
        }

        public void OnRun(InputAction.CallbackContext input)
        {
            if (input.started) _sprintBonus = 2f;
            else if (input.canceled) _sprintBonus = 1f;
        }

        public void OnJump(InputAction.CallbackContext input)
        {
            if (input.started)
            {
                StartJump();
            }
        }

        private void Jump()
        {
            if (_lockedInput) return;
            if (!IsGrounded) return;
            
            Vector3 force;
            if (_movementVector.magnitude > 0.1f) force = Vector3.up + Vector3.forward * .5f;
            else force = Vector3.up;
            
            _rb.AddForce(force.normalized * jumpForce, ForceMode.VelocityChange);
            //_isGrounded = false;
        }
        
        public void OnLeftClick(InputAction.CallbackContext input)
        {
            if (input.started)
            {
                if (!_owningPlayer.IsPickupValid) return;
                _owningPlayer.DropPickup();
            }
            else if (input.canceled) Debug.Log("Let go of LeftClick");
        }
        
        public void OnRightClick(InputAction.CallbackContext input)
        {
            if (input.started) Debug.Log("pressed RightClick");
            else if (input.canceled) Debug.Log("Let go of RightClick");
        }
        
        private void OnDrawGizmos()
        {/*
            //Debug.DrawRay(transform.position, -Vector3.up * groundCheckSize, Color.yellow);
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Vector3 checkPosition = transform.position + groundCheckOffset;
            Gizmos.DrawWireSphere(checkPosition, groundCheckSize);*/
        }
    }
}