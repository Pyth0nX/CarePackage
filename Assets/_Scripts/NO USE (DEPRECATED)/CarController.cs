using UnityEngine;

public class CarController : MonoBehaviour
{
    public float laneOffset = 3f; 
    public float moveSpeed = 10f; 

    private int currentLane = 2; 
    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            MoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            MoveRight();
        }
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);

    }

    void MoveLeft()
    {
        if (currentLane > 1)
        {
            currentLane--;
            UpdateTargetPosition();
        }
    }

    void MoveRight()
    {
        if (currentLane < 3)
        {
            currentLane++;
            UpdateTargetPosition();
        }
    }

    void UpdateTargetPosition()
    {
        targetPosition = new Vector3((currentLane - 2) * laneOffset, transform.position.y, transform.position.z);
    }
}