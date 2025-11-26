using System;
using UnityEngine;

public class Pedestrian : MonoBehaviour
{
    [SerializeField] private float distance = 15f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float minDistance = 0.1f;
    
    private Vector3 _targetLocation;

    private void Start()
    {
        _targetLocation = GetNewTargetLocation();
        //InvokeRepeating("PerformWalking", 0f, 1f);
    }

    private void Update()
    {
        PerformWalking();
    }

    private Vector3 GetNewTargetLocation() => transform.position + transform.forward * distance;
    
    private void PerformWalking()
    {
        if (Vector3.Distance(transform.position, _targetLocation) > minDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetLocation, speed * Time.deltaTime);
            return;
        }

        transform.Rotate(0f, 180f, 0f);
        _targetLocation = GetNewTargetLocation();
    }
}
