using UnityEngine;

public class TextureScroller : MonoBehaviour
{
    public float scrollSpeed = 0.5f;
    [SerializeField] private Renderer rend;
    private int localId = -340396;
    

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        float offset = Time.time * scrollSpeed;
        rend.material.SetTextureOffset(localId, new Vector2(offset, 2));
    }
}