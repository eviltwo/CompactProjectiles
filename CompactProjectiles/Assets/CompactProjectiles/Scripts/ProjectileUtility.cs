using UnityEngine;

namespace CompactProjectiles
{
    public static class ProjectileUtility
    {
        public static void Trace(LaunchData data, float deltaTime, out Vector3 position, out Quaternion rotation)
        {
            position = TraceLaunchedPosition(data.Position, data.Velocity, data.Gravity, deltaTime);
            rotation = TraceLaunchedRotation(data.Rotation, data.AngularVelocity, data.AngularDrag, deltaTime);
        }

        public static Vector3 TraceLaunchedPosition(Vector3 position, Vector3 velocity, float gravity, float deltaTime)
        {
            return position + velocity * deltaTime + Vector3.up * (0.5f * gravity * deltaTime * deltaTime);
        }

        public static float SimulationTimeStep = 0.02f;
        public static int IterationSafetyLimit = 10000;
        public static Quaternion TraceLaunchedRotation(Quaternion rotation, Vector3 angularVelocity, float angularDrag, float deltaTime)
        {
            if (angularVelocity.sqrMagnitude == 0)
            {
                return rotation.normalized;
            }

            var axis = angularVelocity.normalized;
            var t = 0f;
            for (var i = 0; i < IterationSafetyLimit; i++)
            {
                t += SimulationTimeStep;
                var stepDrag = Mathf.Clamp01(angularDrag * SimulationTimeStep);
                angularVelocity -= angularVelocity * stepDrag;
                rotation = Quaternion.AngleAxis(angularVelocity.magnitude * Mathf.Rad2Deg * SimulationTimeStep, axis) * rotation;
                if (t + SimulationTimeStep >= deltaTime)
                {
                    break;
                }
            }

            var remainT = deltaTime - t;
            if (remainT > 0)
            {
                angularVelocity -= angularVelocity * angularDrag * remainT;
                rotation = Quaternion.AngleAxis(angularVelocity.magnitude * Mathf.Rad2Deg * remainT, axis) * rotation;
            }

            return rotation.normalized;
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
    }
}
