using UnityEngine.InputSystem;
using UnityEngine;
using System;

namespace CarePackage.Main
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private GameObject playerCamera;
        [SerializeField] private GameObject sliderPrefab;
        [SerializeField] private float speed = 5f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float jumpForwardBias = 0.5f;
        [SerializeField] private float gravity = 30f;
        [SerializeField, Range(0.01f, 4f)] private float sensitivityX = 1f;
        [SerializeField, Range(0.01f, 4f)] private float sensitivityY = 1f;
        [SerializeField, Range(0.05f, 0.5f)] private float scrollSensitivity = 0.1f;
        [SerializeField] private float groundCheckSize;
        [SerializeField] private Vector3 groundCheckOffset;
        [SerializeField] private bool debug;
        [SerializeField] private bool startLocked = true;
        
        public bool IsGrounded => _isGrounded;
        public PlayerState OwningPlayer => _owningPlayer;

        // private components
        private PlayerState _owningPlayer;
        private CapsuleCollider _collider;
        private PlayerInput _playerInput;
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
        private float _airControlRate = 2f;
        private bool _lockedInput;
        private bool _isGrounded;
        private bool _isJumping;

        public static event Action OnPlayerMoved = delegate {};
        
        public void LockInput(bool lockMode) => _lockedInput = lockMode;
        
        private void Start()
        {
            InitializeAllComponents();
            LoadSensitivity(UI.SensitivitySetting.ESensitivity.LookX);
            LoadSensitivity(UI.SensitivitySetting.ESensitivity.LookY);
            LoadSensitivity(UI.SensitivitySetting.ESensitivity.Scroll);
        }

        private void InitializeAllComponents()
        {
            _rb = transform.root.GetComponent<Rigidbody>();
            _owningPlayer = GameManager.Instance.Player;
            _collider = GetComponent<CapsuleCollider>();
            _playerInput = GetComponent<PlayerInput>();
            CalibrateGroundCheck();
        }

        private void CalibrateGroundCheck()
        {
            _groundRay = new RaycastSensor(transform);
            _groundRay.SetCastOrigin(_collider.bounds.center);
            _groundRay.SetCastDirection(RaycastSensor.CastDirection.Down);
            var castStrategy = new SphereCast(groundCheckSize);
            _groundRay.SetCastStrategy(castStrategy);
            _groundRay.castLength = groundCheckOffset.y;
            _groundRay.layermask = 1;
        }

        private void OnEnable()
        {
            Invoke("Enable", .1f);
        }

        private void Enable()
        {
            UI.UIManager.OnInterfaceOpened += OnInterfaceOpened_Implementation;
            UI.UIManager.OnInterfaceClosed += ListenToUIChanges;
            LockCursor(CursorLockMode.Locked);
        }

        private void OnDisable()
        {
            UI.UIManager.OnInterfaceOpened -= OnInterfaceOpened_Implementation;
            UI.UIManager.OnInterfaceClosed -= ListenToUIChanges;
            LockCursor(CursorLockMode.None);
        }

        private void OnInterfaceOpened_Implementation()
        {
            ListenToUIChanges(true);
        }

        private void ListenToUIChanges(bool uiChange)
        {
            LockInput(uiChange);
            LockCursor(uiChange ? CursorLockMode.None : CursorLockMode.Locked);
            SetInputSchema(uiChange ? "UI" : "Player");
        }
        
        public void SetInputSchema(string schema) => _playerInput.SwitchCurrentActionMap(schema);

        private void Update()
        {
            if (_lockedInput) return;
            
            HandleLook();
        }

        private void FixedUpdate()
        {
            if (_lockedInput) return;
            if (_rb == null) return;
            
            GroundCheck();
            HandleMomentum();
        }

        private void CalculateInputVelocity()
        {
            var inputMoveVector = new Vector3(_inputVector.x, 0, _inputVector.y);
            var moveDirection = transform.TransformDirection(inputMoveVector);
            if (moveDirection.magnitude > 1f) moveDirection.Normalize();

            _velocity = speed * _sprintBonus * moveDirection;
        }

        private void HandleMomentum()
        {
            CalculateInputVelocity();
            _momentum = transform.localToWorldMatrix * _momentum;

            var verticalMomentum = VectorMath.ExtractDotVector(_momentum, transform.up);
            var horizontalMomentum = _momentum - verticalMomentum;
            
            verticalMomentum -=  Time.fixedDeltaTime * gravity * transform.up;
            
            if (IsGrounded && VectorMath.GetDotProduct(verticalMomentum, transform.up) < 0f)
                verticalMomentum = Vector3.zero;
            
            if (!IsGrounded)
                AdjustHorizontalMomentum(ref horizontalMomentum, _velocity);
            
            var friction = IsGrounded ? 100 : .5f;
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, friction * Time.fixedDeltaTime);

            if (_isJumping)
            {
                var jumpDirection = _velocity.normalized * jumpForwardBias + transform.up;
                verticalMomentum = jumpDirection.normalized * jumpForce;
            }
            
            _momentum = horizontalMomentum + verticalMomentum;
            _momentum = transform.worldToLocalMatrix * _momentum;
            /*
            Vector3 targetPos = _rb.position + (_velocity + _momentum) * Time.fixedDeltaTime;
            _rb.MovePosition(targetPos);*/
            _rb.linearVelocity = _velocity + _momentum;
        }
        
        Vector3 CalculateMovementDirection() 
        {
            Vector3 direction = transform.right * _inputVector.x + transform.forward * _inputVector.y;
            return direction.magnitude > 1f ? direction.normalized : direction;
        }

        private void AdjustHorizontalMomentum(ref Vector3 horizontalMomentum, Vector3 verticalMomentum)
        {/*
            if (horizontalMomentum.magnitude > speed) {
                if (VectorMath.GetDotProduct(_movementVector, horizontalMomentum.normalized) > 0f) {
                    _movementVector = VectorMath.RemoveDotVector(_movementVector, horizontalMomentum.normalized);
                }
                horizontalMomentum += _movementVector * (Time.deltaTime * airControlRate * 0.25f);
            }
            else {
                horizontalMomentum += _movementVector * (Time.deltaTime * airControlRate);
                horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, speed);
            }*/
            
            Vector3 inputDirection = new Vector3(_inputVector.x, 0, _inputVector.y);
            inputDirection = transform.TransformDirection(inputDirection);

            if (horizontalMomentum.magnitude > speed)
            {
                if (VectorMath.GetDotProduct(inputDirection, horizontalMomentum.normalized) > 0f)
                {
                    inputDirection = VectorMath.RemoveDotVector(inputDirection, horizontalMomentum.normalized);
                }
                horizontalMomentum += inputDirection * (Time.deltaTime * _airControlRate * 0.25f);
            }
            else
            {
                horizontalMomentum += inputDirection * (Time.deltaTime * _airControlRate);
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
        }

        private void GroundCheck()
        {
            _isGrounded = _groundRay.HasDetectedHitStatic();
        }

        private void HandleLook()
        {
            _horizontalCameraSpeed = _lookVector.x * sensitivityX;
            _verticalCameraSpeed = _lookVector.y * sensitivityY;
            
            _yRotation = _horizontalCameraSpeed;
            
            _xRotation -= _verticalCameraSpeed;
            _xRotation = Mathf.Clamp(_xRotation, -90, 60);
            if (playerCamera != null) playerCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
            
            _rb.MoveRotation(_rb.rotation * Quaternion.AngleAxis(_yRotation, Vector3.up));
        }

        public void PreviewSensitivity(UI.SensitivitySetting.ESensitivity sensitivityCategory, float newValue)
        {
            switch (sensitivityCategory)
            {
                case UI.SensitivitySetting.ESensitivity.LookX:
                    sensitivityX = newValue;
                    break;
                case UI.SensitivitySetting.ESensitivity.LookY:
                    sensitivityY = newValue;
                    break;
                case UI.SensitivitySetting.ESensitivity.Scroll:
                    scrollSensitivity = newValue;
                    break;
                default:
                    break;
            }
        }

        public void SaveSensitivity(UI.SensitivitySetting.ESensitivity sensitivityCategory, float newValue)
        {
            PlayerPrefs.SetFloat(sensitivityCategory.ToString(), newValue);
            LoadSensitivity(sensitivityCategory);
        }

        public void LoadSensitivity(UI.SensitivitySetting.ESensitivity sensitivityCategory)
        {
            switch (sensitivityCategory)
            {
                case UI.SensitivitySetting.ESensitivity.LookX:
                    sensitivityX = GetSensitivity(UI.SensitivitySetting.ESensitivity.LookX);
                    break;
                case UI.SensitivitySetting.ESensitivity.LookY:
                    sensitivityY = GetSensitivity(UI.SensitivitySetting.ESensitivity.LookY);
                    break;
                case UI.SensitivitySetting.ESensitivity.Scroll:
                    scrollSensitivity = GetSensitivity(UI.SensitivitySetting.ESensitivity.Scroll);
                    break;
                default:
                    break;
            }
        }
        
        public float GetSensitivity(UI.SensitivitySetting.ESensitivity sensitivityCategory)
        {
            var sens = PlayerPrefs.GetFloat(sensitivityCategory.ToString(), 1);
            return sens;
        }
        
        private void LockCursor(CursorLockMode lockMode)
        {
            Cursor.lockState = lockMode;
            Cursor.visible = lockMode == CursorLockMode.None;
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

        private float _inputLeftHeldTime;
        private float _inputRightHeldTime;

        private Interaction.Miscellaneous.TweenSliderAction _sliderTween;
        private PrimeTween.Tween _delayTween;
        
        public void OnLeftClick(InputAction.CallbackContext input)
        {
            if (input.started)
            {
                if (!_owningPlayer.IsPickupValid) return;
                _inputLeftHeldTime = Time.time;
                _delayTween = PrimeTween.Tween.Delay(0.6f).OnComplete(() =>
                {
                    if (_owningPlayer.IsPickupValid) // Still valid after delay?
                    {
                        _sliderTween = new Interaction.Miscellaneous.TweenSliderAction(sliderPrefab);
                        _sliderTween.StartTweening();
                    }
                });
            }
            else if (input.canceled)
            {
                Debug.Log("Let go of LeftClick");
                if (!_owningPlayer.IsPickupValid) return;
                var heldDuration = Time.time - _inputLeftHeldTime;
                
                _delayTween.Stop();
                _delayTween = default;
                
                _sliderTween?.StopTweening();
                _sliderTween = null;
                
                _owningPlayer.LaunchPickup(heldDuration);
            }
        }
        
        public void OnRightClick(InputAction.CallbackContext input)
        {
            if (input.started) Debug.Log("pressed RightClick");
            else if (input.canceled) Debug.Log("Let go of RightClick");
        }
        
        public void OnScroll(InputAction.CallbackContext context)
        {
            Vector2 scrollDelta = context.ReadValue<Vector2>();
            if (_owningPlayer == null) return;
            _owningPlayer.ChangePickupDistance(scrollDelta.y * scrollSensitivity);
        }
        
        private void OnDrawGizmos()
        {
            if (debug && _groundRay != null) _groundRay.DrawDebug();
        }
    }
}