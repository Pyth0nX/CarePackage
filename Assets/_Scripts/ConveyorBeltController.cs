using UnityEngine;

public class ConveyorBeltController : MonoBehaviour
{
    [SerializeField] private Renderer renderer;

    void Start()
    {
        renderer.material = new Material(renderer.material);
        SetSpeed(0f);
    }

    public void SetSpeed(float speed)
    {
        renderer.material.SetFloat("T_Speed", speed);
    }
    
}
