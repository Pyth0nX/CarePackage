using CarePackage.Utilities;
using UnityEngine;

public class TestingPositioning : MonoBehaviour
{
    [SerializeField] private Transform center;
    [SerializeField] private float offset;
    [SerializeField] private int count;
    [SerializeField] private Vector3 size = new(1.33f, 1.2f, 2f);
    [SerializeField] private GameObject prefab;
    
    private void Start()
    {
        //PositionUtilities.SpawnPrefabInRadialGrid(center, prefab, count, offset);
        //PositionUtilities.SpawnPrefabInGridPattern(center, prefab, count, offset);
        var positions = PositionUtilities.GenerateStrict2x3Grid(center.position, count, offset);
        foreach (var pos in positions)
        {
            Instantiate(prefab, pos, Quaternion.identity);
        }
    }
    
    private void OnDrawGizmos()
    {
        if (prefab == null) return;

        Vector3 center = transform.position;
        float spacing = Mathf.Max(size.x, size.y, size.z) + offset;

        var positions = PositionUtilities.GenerateStrict2x3Grid(center, count, spacing);

        Gizmos.color = Color.cyan;
        foreach (var pos in positions)
        {
            Gizmos.DrawWireCube(pos, size);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(center, 0.05f);
    }
}
