using UnityEngine;

public class Pedestrian : MonoBehaviour
{
    [SerializeField] private float distance;
    [SerializeField] private float speed;
    [SerializeField] private float minDistance = 0.1f;
    
    private Vector3 _targetLocation;

    private void Start()
    {
        _targetLocation = GetNewTargetLocation();
    }

    private void Update()
    {
       PerformWalking(); 
    }

    private Vector3 GetNewTargetLocation()
    {
        return transform.position + transform.forward * distance;
    }

    private void PerformWalking()
    {
        if (Vector3.Distance(transform.position, _targetLocation) > minDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetLocation, speed * Time.deltaTime);
            return;
        }
        Debug.Log("ReachedTarget");

        transform.Rotate(0f, 180f, 0f);
        _targetLocation = GetNewTargetLocation();
    }
}
