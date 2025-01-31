using UnityEngine;

namespace CompactProjectiles
{
    public class LaunchData
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float Gravity;
        public float Duration;
        public bool IsHit;
        public Vector3 HitPoint;
        public Vector3 HitNormal;
        public int IterationCount;
        public int RaycastCount;
    }

    public class BoxProjectile
    {
        [System.Serializable]
        public class ShapeData
        {
            public Vector3 Size = Vector3.one;
        }

        public float Gravity = -9.8f;

        public PhysicsMaterial PhysicsMaterial;

        public Vector3 Position;

        public Vector3 Velocity;

        public float StepAngle = 45f;

        public float ErrorDistance = 0.5f;

        public int MaxRaycastCount = 5;

        public float MaxDuration = 5f;

        public LayerMask LayerMask = -1;

        public BoxProjectile(PhysicsMaterial material)
        {
            PhysicsMaterial = material;
        }

        public LaunchData Simulate()
        {
            var startPosition = Position;
            var startVelocity = Velocity;
            var totalAirTime = 0f;
            var vChanged = false;
            var g = Gravity;
            var r = 0.5f;
            var maxItterations = 1000;
            var raycastCount = 0;
            var raycastSkippedTotalDistance = 0f;
            var iterrationCount = 0;
            for (int i = 0; i < maxItterations; i++)
            {
                iterrationCount++;
                var p = Position;
                var v = Velocity;
                var v2 = new Vector2(new Vector2(v.x, v.z).magnitude, v.y);

                var angle = Mathf.Atan2(v2.y, v2.x) * Mathf.Rad2Deg;
                var stepped_angle = Mathf.Clamp(angle - StepAngle, -90f, 90f);
                var stepped_v2 = Mathf.Sin(stepped_angle * Mathf.Deg2Rad) * (v2.x / Mathf.Cos(stepped_angle * Mathf.Deg2Rad));
                var stepped_t = (stepped_v2 - v2.y) / g;
                if (stepped_t <= 0f)
                {
                    stepped_t = 1;
                }
                stepped_t = Mathf.Clamp(stepped_t, 0f, 1f);

                var stepped_p = p + v * stepped_t + Vector3.up * (0.5f * g * stepped_t * stepped_t);

                var raycastRequired = false;
                var stepped_v = v + Vector3.up * g * stepped_t;
                if (!vChanged && Vector3.Dot(stepped_v, startVelocity) < 0)
                {
                    vChanged = true;
                    raycastRequired = true;
                }

                var toSteppedVec = stepped_p - p;
                var dir = toSteppedVec.normalized;
                var dist = toSteppedVec.magnitude;
                raycastSkippedTotalDistance += dist;
                raycastRequired |= raycastSkippedTotalDistance > ErrorDistance;
                Debug.DrawRay(stepped_p, Vector3.up, raycastRequired ? Color.red : Color.green);
                if (raycastRequired)
                {
                    raycastSkippedTotalDistance = 0f;
                    raycastCount++;
                    if (Physics.SphereCast(p, r, dir, out var hit, dist, LayerMask))
                    {
                        var ref_p = hit.point + hit.normal * (r + 0.01f);
                        var ref_diff_st = ref_p - startPosition;
                        var ref_diff2_st = new Vector2(new Vector2(ref_diff_st.x, ref_diff_st.z).magnitude, ref_diff_st.y);
                        var ref_t = ref_diff2_st.x / v2.x;
                        var v0y = (ref_diff2_st.y - 0.5f * g * ref_t * ref_t) / ref_t;
                        var launchData = new LaunchData
                        {
                            Position = startPosition,
                            Velocity = new Vector3(startVelocity.x, v0y, startVelocity.z),
                            Gravity = g,
                            Duration = ref_t,
                            IsHit = true,
                            HitPoint = hit.point,
                            HitNormal = hit.normal,
                            IterationCount = iterrationCount,
                            RaycastCount = raycastCount,
                        };
                        Position = ref_p;
                        Velocity = launchData.Velocity + Vector3.up * g * ref_t;
                        Velocity = Vector3.Reflect(Velocity, hit.normal);
                        var hitMat = hit.collider.sharedMaterial ?? PhysicsMaterial;
                        var frictionCombine = ProjectileUtility.MergePhysicsMaterialCombine(PhysicsMaterial.frictionCombine, hitMat.frictionCombine);
                        var friction = ProjectileUtility.CombineFriction(frictionCombine, PhysicsMaterial.dynamicFriction, hitMat.dynamicFriction);
                        Velocity -= Vector3.ProjectOnPlane(Velocity, Vector3.up) * friction;
                        var bounceCombine = ProjectileUtility.MergePhysicsMaterialCombine(PhysicsMaterial.bounceCombine, hitMat.bounceCombine);
                        var bounce = ProjectileUtility.CombineFriction(bounceCombine, PhysicsMaterial.bounciness, hitMat.bounciness);
                        Velocity -= Vector3.Project(Velocity, hit.normal) * (1 - bounce);
                        return launchData;
                    }
                }

                Position = stepped_p;
                Velocity += Vector3.up * g * stepped_t;
                totalAirTime += stepped_t;
                if (raycastCount >= MaxRaycastCount)
                {
                    break;
                }

                if (totalAirTime >= MaxDuration)
                {
                    break;
                }
            }

            return new LaunchData
            {
                Position = startPosition,
                Velocity = startVelocity,
                Gravity = g,
                Duration = totalAirTime,
                IsHit = false,
                IterationCount = iterrationCount,
                RaycastCount = raycastCount,
            };
        }
    }

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
    }
}