using UnityEngine;

namespace CompactProjectiles
{
    public class ProjectileController : MonoBehaviour
    {
        public BoxProjectile.ShapeData ShapeData;

        public PhysicsMaterial PhysicsMaterial;

        public float StepAngle = 45f;

        public float ErrorDistance = 0.5f;

        public float MaxDuration = 5f;

        public int MaxRaycastCount = 5;

        public LayerMask LayerMask = -1;

        public Vector3 StartVelocity = Vector3.right;

        public Vector3 StartAngularVelocity = Vector3.zero;

        public float AngularDrag = 0.1f;

        private BoxProjectile _projectile;

        private LaunchData _lastLaunchData;
        private float _animElapsedTime;

        public bool IsSleep => _projectile.IsSleep;

        public void Launch(Vector3 velocity, Vector3 angularVelocity)
        {
            _projectile.Position = transform.position;
            _projectile.Velocity = velocity;
            _projectile.Rotation = transform.rotation;
            _projectile.AngularVelocity = angularVelocity;
            _projectile.IsSleep = false;
        }

        private void Start()
        {
            ShapeData.Size = Vector3.Scale(ShapeData.Size, transform.localScale);
            _projectile = new BoxProjectile(ShapeData, PhysicsMaterial);
            _projectile.Position = transform.position;
            _projectile.Velocity = StartVelocity;
            _projectile.Rotation = transform.rotation;
            _projectile.AngularVelocity = StartAngularVelocity;
        }

        private void Update()
        {
            _animElapsedTime += Time.deltaTime;
            if (_lastLaunchData == null || (!_projectile.IsSleep && _animElapsedTime >= _lastLaunchData.Duration))
            {
                var duration = _lastLaunchData?.Duration ?? 0;
                _animElapsedTime -= duration > Time.deltaTime ? duration : _animElapsedTime;
                _projectile.AngularDrag = AngularDrag;
                _projectile.StepAngle = StepAngle;
                _projectile.ErrorDistance = ErrorDistance;
                _projectile.MaxDuration = MaxDuration;
                _projectile.MaxRaycastCount = MaxRaycastCount;
                _projectile.LayerMask = LayerMask;
                _lastLaunchData = _projectile.Simulate();
            }

            if (_projectile.IsSleep)
            {
                transform.SetPositionAndRotation(_projectile.Position, _projectile.Rotation);
            }
            else
            {
                ProjectileUtility.Trace(_lastLaunchData, _animElapsedTime, out var p, out var r);
                transform.SetPositionAndRotation(p, r);
            }
        }

        private static Color SleepColor = new Color(0f, 0f, 0.5f, 0.5f);
        private static Color ActiveColor = new Color(0.5f, 0f, 0f, 0.5f);
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Gizmos.color = _projectile.IsSleep ? SleepColor : ActiveColor;
            BulgingBoxUtility.DrawBoxGizmos(_projectile.Box, 5);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_projectile.LastSimulationState.HitPosition, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(_projectile.LastSimulationState.HitPosition, _projectile.LastSimulationState.HitNormal);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(_projectile.LastSimulationState.HitPosition, _lastLaunchData.Velocity);
        }
    }
}
