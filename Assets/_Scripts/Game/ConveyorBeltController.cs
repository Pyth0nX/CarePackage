using UnityEngine;

public class ConveyorBeltController : MonoBehaviour
{
    [SerializeField] private Renderer render;

    void Start()
    {
        render.material = new Material(render.material);
        SetSpeed(0f);
    }

    public void SetSpeed(float speed)
    {
        render.material.SetFloat("T_Speed", speed);
    }
}
