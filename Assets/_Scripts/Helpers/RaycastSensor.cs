using UnityEngine;

namespace CarePackage.Main
{
    public interface ICastStrategy
    {
        bool Cast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float distance, int layerMask);
        bool StaticCast(Vector3 origin, Vector3 direction, float distance, int layerMask);
        void DrawDebug(Vector3 origin, Vector3 direction, float distance, Color color);
    }
    
    [System.Serializable]
    public class RayCast : ICastStrategy
    {
        private QueryTriggerInteraction _queryType;
        
        public RayCast() : this(QueryTriggerInteraction.Ignore) {}
        
        public RayCast(QueryTriggerInteraction inQueryType)
        {
            _queryType = inQueryType;
        }
        
        public bool Cast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float distance, int layerMask)
        {
            return Physics.Raycast(origin, direction, out hitInfo, distance, layerMask, _queryType);
        }

        public bool StaticCast(Vector3 origin, Vector3 direction, float distance, int layerMask)
        {
            return Physics.Raycast(origin, direction, distance, layerMask, _queryType);
        }

        public void DrawDebug(Vector3 origin, Vector3 direction, float distance, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(origin, direction * distance);
        }
    }

    [System.Serializable]
    public class SphereCast : ICastStrategy
    {
        private float _radius;
        private QueryTriggerInteraction _queryType;
        
        public SphereCast() : this(1f) {}
        
        public SphereCast(float inRadius) : this(inRadius, QueryTriggerInteraction.Ignore) {}
        
        public SphereCast(float inRadius, QueryTriggerInteraction inQueryType)
        {
            _radius = inRadius;
            _queryType = inQueryType;
        }
        
        public bool Cast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float distance, int layerMask)
        {
            return Physics.SphereCast(origin, _radius, direction, out hitInfo, distance, layerMask);
        }

        public bool StaticCast(Vector3 origin, Vector3 direction, float distance, int layerMask)
        {
            var checkPosition = origin + direction * distance;
            return Physics.CheckSphere(checkPosition, _radius, layerMask, _queryType);
        }

        public void DrawDebug(Vector3 origin, Vector3 direction, float distance, Color color)
        {
            var checkPosition = origin + direction * distance;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(checkPosition, _radius);
        }
    }
        
    [System.Serializable]
    public class RaycastSensor
    {
        public float castLength = 1f;
        public LayerMask layermask = 255;
        
        [SerializeReference, SerializeReferenceEditor.SR] private ICastStrategy castStrategy;
        
        [SerializeField] public Vector3 _origin = Vector3.zero;
        public Transform _owningTransform;
        
        public enum CastDirection { Forward, Right, Up, Backward, Left, Down }
        private CastDirection _castDirection;
        
        private RaycastHit _hitInfo;
        
        public RaycastSensor(Transform playerTransform) 
        {
            _owningTransform = playerTransform;
        }
        
        public void Cast() 
        {
            if (castStrategy == null) return;
            castStrategy.Cast(WorldOrigin, GetCastDirection(), out _hitInfo, castLength, layermask);
        }

        public bool CastStatic()
        {
            if (castStrategy == null) return false;
            return castStrategy.StaticCast(WorldOrigin, GetCastDirection(), castLength, layermask);
        }
        
        public void SetCastOrigin(Vector3 pos) => _origin = _owningTransform.InverseTransformPoint(pos);
        public void SetCastDirection(CastDirection direction) => _castDirection = direction;
        public void SetCastStrategy(ICastStrategy strategy) => castStrategy = strategy;
        
        public bool HasDetectedHit() => _hitInfo.collider != null;
        public bool HasDetectedHitStatic() => CastStatic();
        public Vector3 WorldOrigin => _owningTransform.TransformPoint(_origin);
        public float GetDistance() => _hitInfo.distance;
        public Vector3 GetNormal() => _hitInfo.normal;
        public Vector3 GetPosition() => _hitInfo.point;
        public Collider GetCollider() => _hitInfo.collider;
        public Transform GetTransform() => _hitInfo.transform;
        
        public Vector3 GetCastDirection() 
        {
            return _castDirection switch 
            {
                CastDirection.Forward => _owningTransform.forward,
                CastDirection.Right => _owningTransform.right,
                CastDirection.Up => _owningTransform.up,
                CastDirection.Backward => -_owningTransform.forward,
                CastDirection.Left => -_owningTransform.right,
                CastDirection.Down => -_owningTransform.up,
                _ => Vector3.one
            };
        }
        
        public void DrawDebug() 
        {
            //if (!HasDetectedHit()) return;
            
            if (castStrategy == null) return;
            Color debugColor = Color.red;
            if (HasDetectedHitStatic()) debugColor = Color.green;
            Vector3 worldOrigin = _owningTransform.TransformPoint(_origin);
            castStrategy.DrawDebug(worldOrigin, GetCastDirection(), castLength, debugColor);
            
            /*
            Debug.DrawRay(_hitInfo.point, _hitInfo.normal, Color.red, Time.deltaTime);
            float markerSize = 0.2f;
            Debug.DrawLine(_hitInfo.point + Vector3.up * markerSize, _hitInfo.point - Vector3.up * markerSize, Color.green, Time.deltaTime);
            Debug.DrawLine(_hitInfo.point + Vector3.right * markerSize, _hitInfo.point - Vector3.right * markerSize, Color.green, Time.deltaTime);
            Debug.DrawLine(_hitInfo.point + Vector3.forward * markerSize, _hitInfo.point - Vector3.forward * markerSize, Color.green, Time.deltaTime);*/
        }
    }
}