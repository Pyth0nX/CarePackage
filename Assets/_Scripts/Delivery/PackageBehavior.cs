using UnityEngine;

namespace CarePackage.Delivery
{
    public enum EPackageState
    {
        Pristine = 0, 
        SlightlyDamaged = 1, 
        Damaged = 2, 
        Flattened = 3, 
        Broken = 4
    }
    
    public class PackageBehavior : MonoBehaviour
    {
        [SerializeField] private Mesh[] meshes;
        [SerializeField] private EPackageState packageState;
        [SerializeField] private float defaultVelocityThreshold = 12;
        [SerializeField] private float heldVelocityThreshold = 5;
        
        public float VelocityThreshold { get => _velocityThreshold; set => _velocityThreshold = value; }
        public float DefaultVelocityThreshold { get => defaultVelocityThreshold; set => defaultVelocityThreshold = value; }
        public float HeldVelocityThreshold { get => heldVelocityThreshold; set => heldVelocityThreshold = value; }
        
        private MeshFilter _meshFilter;
        private Rigidbody _rigidbody;
        private UnityEngine.UI.Image[] _packageItemImages;
        private float _velocityThreshold;
        private int _currentMeshIndex;
        private bool _canBeDamaged;
        private bool _usingGravity;
        public float durability = 100;
        private float _durability;
        public float fragility = .8f;
        
        public event System.Action<EPackageState, EPackageState> OnStateChanged;

        private void Awake()
        {
            FetchComponents();
            _velocityThreshold = DefaultVelocityThreshold;
        }

        private void FetchComponents()
        {
            _meshFilter = GetComponentInChildren<MeshFilter>();
            _rigidbody = GetComponent<Rigidbody>();
            _packageItemImages = GetComponentsInChildren<UnityEngine.UI.Image>();
            foreach (var packageImage in _packageItemImages)
            {
                packageImage.enabled = false;
            }
        }

        private void Start()
        {
            SetDamageEnabled(_canBeDamaged);
            UpdateState();
            _durability = durability;
        }
        
        public void SetDamageEnabled(bool damageEnabled, bool togglePhysics = false)
        {
            if (_canBeDamaged == damageEnabled) return;
            _canBeDamaged = damageEnabled;
            if (togglePhysics) TogglePhysics(true);
        }

        public void TogglePhysics(bool toggle)
        {
            if (_rigidbody == null) return;
            
            _usingGravity = toggle;
            _rigidbody.isKinematic = !_usingGravity;
            _rigidbody.useGravity = _usingGravity;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!_canBeDamaged) return;
            
            int otherLayer = other.gameObject.layer;
            if (otherLayer == LayerMask.NameToLayer("Interaction") ||
                otherLayer == LayerMask.NameToLayer("Player") ||
                otherLayer == LayerMask.NameToLayer("UI"))
                return;
            /*
            float impulse = 0f;
            impulse = other.impulse.magnitude;
            //foreach (var cp in other.contacts) impulse += other.impulse.magnitude;
            if (impulse <= 0f) impulse = other.relativeVelocity.magnitude * _rigidbody.mass;
            Debug.Log($"impulse.magnitude: {other.impulse.magnitude} impulse.relativeVel * mass: {other.relativeVelocity.magnitude * _rigidbody.mass}"); 
            // seems like relativeVel * mass is more stable and gives less 200+ numbers*/
            
            /**
             *
             * In Project Settings → Physics:
             * 
             * Increase Default Solver Iterations (12–20).
             * Increase Default Solver Velocity Iterations (6–10).
             * This reduces jitter and impulse spikes.
             */
            
            float impulse = other.relativeVelocity.magnitude * _rigidbody.mass;
            
            float dmg = CalculateDamage(impulse);
            if (dmg <= 0f) return;
            
            _durability -= dmg;
            Damage();
/*
            if (impulse > VelocityThreshold) {
                float dmg = impulse * fragility; // fragility
                _durability -= dmg;
                Damage();
            }*/
            /*
            if (!other.gameObject.CompareTag("Player"))
            {
                Vector3 avgNormal = Vector3.zero;
                foreach (var contact in other.contacts)
                {
                    avgNormal += contact.normal;
                }
                avgNormal.Normalize();

                float dot = Vector3.Dot(avgNormal, transform.up);
                float impact = other.relativeVelocity.magnitude;

                if (dot < -0.5f) Debug.Log("top of package hit");
                else if (dot > 0.5f) Debug.Log("bottom of package hit");
                else Debug.Log("side of package hit");

                if (impact >= _velocityThreshold) Damage();
            }*/
        }
        
        private float CalculateDamage(float rawImpact) => rawImpact switch 
        {
            <= 15f => 0f,
            <= 25f => 5f,
            <= 40f => Mathf.Lerp(12f, 15f, (rawImpact - 25f) / (40f - 25f)),
            <= 300f => Mathf.Lerp(15f, 17f, (rawImpact - 40f) / (300f - 40f)),
            _ => 20f
        };
        
        private float CalculateDamageLoop(float rawImpact) 
        {
            if (rawImpact <= 15f) return 0f;

            (float min, float max, float start, float end)[] ranges = 
            {
                (15f, 25f, 5f, 5f),
                (25f, 40f, 12f, 15f),
                (40f, 300f, 15f, 17f),
                (300f, float.MaxValue, 20f, 20f)
            };

            foreach (var r in ranges) 
            {
                if (rawImpact <= r.max) 
                {
                    float t = (rawImpact - r.min) / (r.max - r.min);
                    return Mathf.Lerp(r.start, r.end, t);
                }
            }
            return 0f;
        }

        public void DamagePackage() => Damage();
        
        public System.Delegate[] GetStateChangedDelegates() => OnStateChanged?.GetInvocationList();

        private void Damage()
        {
            if (packageState == EPackageState.Broken) return;
            var fraction = _durability / durability;
            // Invert so 1 = full, 0 = destroyed
            var damageFraction = 1f - fraction;
            var previousState = packageState;
            
            // Map into 0–4 range
            var stateIndex = Mathf.FloorToInt(damageFraction * System.Enum.GetValues(typeof(EPackageState)).Length);
            stateIndex = Mathf.Clamp(stateIndex, 0, 4);

            packageState = (EPackageState)stateIndex;
            OnStateChanged?.Invoke(previousState, packageState);
            UpdateState();
        }
        
        public void UpdateState()
        {
            int index = (int)packageState;
            _meshFilter.mesh = meshes[index];
            Debug.Log($"Package damaged updated mesh to {packageState}");
        }

        public void SetImage(Sprite icon)
        {
            if (icon == null) return;
            if (_packageItemImages == null || _packageItemImages.Length == 0) return;
            foreach (var packageImage in _packageItemImages)
            {
                packageImage.sprite = icon;
                packageImage.enabled = true;
            }
        }
    }
}