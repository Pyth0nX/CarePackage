using CarePackage.Interaction;
using UnityEngine;
using System;

public class DamagableBehavior : MonoBehaviour, IDamagable
{
    [SerializeField] private float maxHealth = 5;
    [SerializeField] private bool useTrigger = true;
    [SerializeField] private LayerMask damageLayer;

    private BoxCollider _collider;
    private float _velocityThreshold;
    private float _health;
    
    public float VelocityThreshold { get => _velocityThreshold; set => _velocityThreshold = value; }

    public event Action OnDamaged;

    private void Start()
    {
        _health = maxHealth;
        _collider = GetComponent<BoxCollider>();
        _collider.isTrigger = useTrigger;
    }

    public void Damage(float incomingDamage = 1)
    {
        Debug.Log("Damaged " + gameObject.transform.root);
        OnDamaged?.Invoke();
        ModifyHealth(incomingDamage);
    }

    private void ModifyHealth(float incomingValue)
    {
        _health = Mathf.Clamp(_health + incomingValue, 0, maxHealth);
        if (_health <= 0) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useTrigger) return;
        if ((damageLayer.value & (1 << gameObject.layer)) == 0)
        {
            Debug.LogWarning($"[Interactable] Layer mismatch: {gameObject.name} is on layer {gameObject.layer}, not in {damageLayer}");
            return;
        }
        Rigidbody rootRb = transform.root.GetComponent<Rigidbody>();
        if (rootRb == null)
        {
            Debug.LogWarning("No Rigidbody found on root object.");
            return;
        }

        float velocityMagnitude = rootRb.linearVelocity.magnitude;
        if (velocityMagnitude < _velocityThreshold)
        {
            // Too slow — ignore
            return;
        }

        Vector3 direction = rootRb.linearVelocity.normalized;
        float dot = Vector3.Dot(direction, transform.up);
        
        if (dot < -0.5f)
            Damage();
        else if (dot > 0.5f)
            return;
        else
            Damage();
    }
}