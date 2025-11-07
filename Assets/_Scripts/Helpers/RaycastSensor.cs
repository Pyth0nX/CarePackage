using UnityEngine;

namespace CarePackage.Main
{
    public class RaycastSensor
    {
        public float castLength = 1f;
        public LayerMask layermask = 255;
        
        [SerializeField] public Vector3 _origin = Vector3.zero;
        public Transform _owningTransform;
        
        public enum CastDirection { Forward, Right, Up, Backward, Left, Down }
        public CastDirection castDirection;
        
        RaycastHit hitInfo;

        public RaycastSensor(Transform playerTransform) {
            _owningTransform = playerTransform;
        }

        public void Cast() {
            Vector3 worldOrigin = _owningTransform.TransformPoint(_origin);
            Vector3 worldDirection = GetCastDirection();
            
            Physics.Raycast(worldOrigin, worldDirection, out hitInfo, castLength, layermask, QueryTriggerInteraction.Ignore);
        }
        
        public bool HasDetectedHit() => hitInfo.collider != null;
        public float GetDistance() => hitInfo.distance;
        public Vector3 GetNormal() => hitInfo.normal;
        public Vector3 GetPosition() => hitInfo.point;
        public Collider GetCollider() => hitInfo.collider;
        public Transform GetTransform() => hitInfo.transform;
        
        public void SetCastDirection(CastDirection direction) => castDirection = direction;
        public void SetCastOrigin(Vector3 pos) => _origin = _owningTransform.InverseTransformPoint(pos);

        Vector3 GetCastDirection() {
            return castDirection switch {
                CastDirection.Forward => _owningTransform.forward,
                CastDirection.Right => _owningTransform.right,
                CastDirection.Up => _owningTransform.up,
                CastDirection.Backward => -_owningTransform.forward,
                CastDirection.Left => -_owningTransform.right,
                CastDirection.Down => -_owningTransform.up,
                _ => Vector3.one
            };
        }
        
        public void DrawDebug() {
            if (!HasDetectedHit()) return;

            Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red, Time.deltaTime);
            float markerSize = 0.2f;
            Debug.DrawLine(hitInfo.point + Vector3.up * markerSize, hitInfo.point - Vector3.up * markerSize, Color.green, Time.deltaTime);
            Debug.DrawLine(hitInfo.point + Vector3.right * markerSize, hitInfo.point - Vector3.right * markerSize, Color.green, Time.deltaTime);
            Debug.DrawLine(hitInfo.point + Vector3.forward * markerSize, hitInfo.point - Vector3.forward * markerSize, Color.green, Time.deltaTime);
        }
    }
}