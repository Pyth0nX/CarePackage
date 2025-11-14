using UnityEngine;

namespace CarePackage.Delivery
{
    public enum EPackageState { Pristine = 0, SlightlyDamaged = 1, Damaged = 2, Flattened = 3, Broken = 4 }
    
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
        private float _velocityThreshold;
        private int _currentMeshIndex;
        private bool _canBeDamaged = false;
        private bool _usingGravity = false;

        private void Awake()
        {
            FetchComponents();
            _velocityThreshold = DefaultVelocityThreshold;
        }

        private void FetchComponents()
        {
            _meshFilter = GetComponentInChildren<MeshFilter>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            SetDamageEnabled(_canBeDamaged);
            UpdateState();
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
            }
        }

        public void DamagePackage() => Damage();

        private void Damage()
        {
            if (packageState == EPackageState.Broken) return;

            packageState++;
            UpdateState();
            
            Debug.Log($"Package damaged updated mesh to {packageState}");
        }
        
        public void UpdateState()
        {
            int index = (int)packageState;
            _meshFilter.mesh = meshes[index];
        }
    }
}