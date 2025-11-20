using UnityEngine;

public class GizmoCube : MonoBehaviour
{/*
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.95f, 0f, 0.2f);
        Gizmos.matrix = base.transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.95f, 0f, 0.5f);
        Gizmos.matrix = base.transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }*/
    
    private void OnDrawGizmos()
    {
        BoxCollider component = base.GetComponent<BoxCollider>();
        Gizmos.color = new Color(0.4f, 0.19f, 1f);
        Gizmos.matrix = base.transform.localToWorldMatrix;
        Gizmos.DrawWireCube(component.center, component.size);
        Gizmos.color = new Color(0.9f, 0.22f, 1f, 0.2f);
        Gizmos.DrawCube(component.center, component.size);
    }
}
