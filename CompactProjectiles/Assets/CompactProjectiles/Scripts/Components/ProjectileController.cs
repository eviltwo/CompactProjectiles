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

        public Vector3 Velocity = Vector3.right;

        public Vector3 AngularVelocity = Vector3.zero;

        private BoxProjectile _projectile;

        public LayerMask LayerMask = -1;

        private LaunchData _lastLaunchData;
        private float _animElapsedTime;

        private void Awake()
        {
            ShapeData.Size = Vector3.Scale(ShapeData.Size, transform.localScale);
            _projectile = new BoxProjectile(ShapeData, PhysicsMaterial);
            _projectile.Position = transform.position;
            _projectile.Velocity = Velocity;
            _projectile.Rotation = transform.rotation;
            _projectile.AngularVelocity = AngularVelocity;
        }

        private void Update()
        {
            _animElapsedTime += Time.deltaTime;
            if (_lastLaunchData == null || _animElapsedTime >= _lastLaunchData.Duration)
            {
                _animElapsedTime -= _lastLaunchData?.Duration ?? 0;
                _projectile.StepAngle = StepAngle;
                _projectile.ErrorDistance = ErrorDistance;
                _projectile.MaxDuration = MaxDuration;
                _projectile.MaxRaycastCount = MaxRaycastCount;
                _projectile.LayerMask = LayerMask;
                _lastLaunchData = _projectile.Simulate();
            }

            if (_lastLaunchData.IsSleep)
            {
                transform.position = _projectile.Position;
                transform.rotation = _projectile.Rotation;
            }
            else
            {
                ProjectileUtility.LaunchSimulation(_lastLaunchData, _animElapsedTime, out var p);
                transform.position = p;
                var angularVelocity = _lastLaunchData.AngularVelocity;
                transform.rotation = ProjectileUtility.ApplyAngularVelocity(_lastLaunchData.Rotation, angularVelocity, _animElapsedTime);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Gizmos.color = _lastLaunchData.IsSleep ? Color.blue : Color.red;
            BulgingBoxUtility.DrawBoxGizmos(_projectile.Box, 5);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_projectile.LastSimulationState.BoxHitPosition, 0.1f);
        }
    }
}
