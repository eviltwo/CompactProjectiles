using UnityEngine;

namespace CompactProjectiles
{
    public static class ProjectileUtility
    {
        public static void LaunchSimulation(LaunchData data, float deltaTime, out Vector3 position)
        {
            position = data.Position + data.Velocity * deltaTime + Vector3.up * (0.5f * data.Gravity * deltaTime * deltaTime);
        }

        public static PhysicsMaterialCombine MergePhysicsMaterialCombine(PhysicsMaterialCombine a, PhysicsMaterialCombine b)
        {
            if (a == PhysicsMaterialCombine.Maximum || b == PhysicsMaterialCombine.Maximum)
            {
                return PhysicsMaterialCombine.Maximum;
            }

            if (a == PhysicsMaterialCombine.Multiply || b == PhysicsMaterialCombine.Multiply)
            {
                return PhysicsMaterialCombine.Multiply;
            }

            if (a == PhysicsMaterialCombine.Minimum || b == PhysicsMaterialCombine.Minimum)
            {
                return PhysicsMaterialCombine.Minimum;
            }

            return PhysicsMaterialCombine.Average;
        }

        public static float CombineFriction(PhysicsMaterialCombine combine, float a, float b)
        {
            switch (combine)
            {
                case PhysicsMaterialCombine.Maximum:
                    return Mathf.Max(a, b);
                case PhysicsMaterialCombine.Multiply:
                    return a * b;
                case PhysicsMaterialCombine.Minimum:
                    return Mathf.Min(a, b);
                case PhysicsMaterialCombine.Average:
                    return (a + b) * 0.5f;
                default:
                    return 0f;
            }
        }

        public static Quaternion ApplyAngularVelocity(Quaternion rotation, Vector3 angularVelocity, float deltaTime)
        {
            if (angularVelocity.sqrMagnitude == 0)
            {
                return rotation;
            }

            var axis = angularVelocity.normalized;
            var angle = angularVelocity.magnitude * Mathf.Rad2Deg * deltaTime;
            return Quaternion.AngleAxis(angle, axis) * rotation;
        }
    }
}
