using UnityEngine;

namespace CompactProjectiles
{
    public class ProjectileController : MonoBehaviour
    {
        public PhysicsMaterial PhysicsMaterial;

        public float StepAngle = 45f;

        public float ErrorDistance = 0.5f;

        public float MaxDuration = 5f;

        public int MaxRaycastCount = 5;

        public Vector3 Velocity = Vector3.right;

        private BoxProjectile _projectile;

        public LayerMask LayerMask = -1;

        private LaunchData _lastLaunchData;
        private float _animElapsedTime;

        private void Awake()
        {
            _projectile = new BoxProjectile(PhysicsMaterial);
            _projectile.Position = transform.position;
            _projectile.Velocity = Velocity;
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
            }
            else
            {
                ProjectileUtility.LaunchSimulation(_lastLaunchData, _animElapsedTime, out var p);
                transform.position = p;
            }
        }
    }
}
