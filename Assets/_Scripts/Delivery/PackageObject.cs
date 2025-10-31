using System;
using UnityEngine;

namespace CarePackage.Delivery
{
    public enum EPackageState { Pristine = 0, SlightlyDamaged = 1, Damaged = 2, Flattened = 3, Broken = 4 }
    
    public class PackageObject : MonoBehaviour
    {
        [SerializeField] private Mesh[] meshes;
        [SerializeField] private GameObject[] meshesObj;
        [SerializeField] private EPackageState packageState;
        [SerializeField] private float defaultVelocityThreshold = 12;
        [SerializeField] private float heldVelocityThreshold = 5;
        [SerializeField] private bool canBeDamaged = false;
        
        public float VelocityThreshold { get => _velocityThreshold; set => _velocityThreshold = value; }
        public float DefaultVelocityThreshold { get => defaultVelocityThreshold; set => defaultVelocityThreshold = value; }
        public float HeldVelocityThreshold { get => heldVelocityThreshold; set => heldVelocityThreshold = value; }
        
        private MeshFilter _meshFilter;
        private Rigidbody _rigidbody;
        private float _velocityThreshold;
        private int _currentMeshIndex;

        private void Start()
        {
            _meshFilter = GetComponentInChildren<MeshFilter>();
            _rigidbody = GetComponent<Rigidbody>();
            UpdateState();
            SetDamageEnabled(canBeDamaged);
        }
        
        public void SetDamageEnabled(bool damageEnabled)
        {
            canBeDamaged = damageEnabled;
        }

        public void TogglePhysics(bool toggle)
        {
            if (_rigidbody == null) return;
            _rigidbody.isKinematic = !toggle;
            _rigidbody.useGravity = toggle;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!canBeDamaged) return;
            
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

        private void Damage()
        {
            if (packageState == EPackageState.Broken) return;

            packageState++;
            //UpdateState();
            
            Debug.Log($"Package damaged updated mesh to {packageState}");
        }
        
        public void UpdateState()
        {
            int index = (int)packageState;
            
            for (int i = 0; i < meshesObj.Length; i++)
            {
                meshesObj[i].SetActive(i == index);
            }

            if (index < meshes.Length)
            {
                _meshFilter.mesh = meshes[index];
            }
        }
    }
}