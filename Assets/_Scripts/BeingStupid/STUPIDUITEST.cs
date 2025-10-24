using UnityEngine;

public class STUPIDUITEST : MonoBehaviour
{
    [SerializeField] private GameObject obj;
    
    public void SetObject(GameObject obj) => this.obj = obj;
    
    public static STUPIDUITEST Instance;

    private void FixedUpdate()
    {
        transform.position = Camera.main.WorldToScreenPoint(obj.transform.position);
    }
}
