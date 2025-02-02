using System.Collections.Generic;
using UnityEngine;

namespace CompactProjectiles
{
    public class LaunchData
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public Quaternion Rotation;
        public Vector3 AngularVelocity;
        public float Gravity;
        public float Duration;
    }

    public class BoxProjectile
    {
        [System.Serializable]
        public class ShapeData
        {
            public Vector3 Size = Vector3.one;

            public float XPositiveBulge;

            public float XNegativeBulge;

            public float YPositiveBulge;

            public float YNegativeBulge;

            public float ZPositiveBulge;

            public float ZNegativeBulge;
        }

        public float Gravity = -9.8f;

        public ShapeData Shape;

        public PhysicsMaterial PhysicsMaterial;

        public Vector3 Position;

        public Vector3 Velocity;

        public Quaternion Rotation = Quaternion.identity;

        // Angular velocity in radian per second
        // vQ = ƒÖ * rMQ
        // vQ : velocity of point Q
        // ƒÖ : angular velocity
        // rMQ : vector from M to Q (M is the instant center of rotation)
        public Vector3 AngularVelocity;

        public float StepAngle = 45f;

        public float ErrorDistance = 1f;

        public int MaxRaycastCount = 5;

        public float MaxDuration = 2f;

        public LayerMask LayerMask = -1;

        public float SpaceToWall = 0.1f;

        public float SleepPositionThreshold = 0.01f;

        public float SleepVelocityThreshold = 0.1f;

        public bool IsSleep;

        private SimulationState _state = new SimulationState();
        public SimulationState LastSimulationState => _state;

        private BulgingBox _box = new BulgingBox();
        public BulgingBox Box => _box;

        public class SimulationState
        {
            public int IterationCount;
            public int RaycastCount;
            public List<Vector3> RaycastPositionLog = new List<Vector3>();
            public bool IsHit;
            public Vector3 HitPosition;
            public Vector3 HitNormal;
            public void Clear()
            {
                IterationCount = 0;
                RaycastCount = 0;
                RaycastPositionLog.Clear();
                IsHit = false;
            }
        }

        public BoxProjectile(ShapeData shapeData, PhysicsMaterial material)
        {
            Shape = shapeData;
            _box.Scale = shapeData.Size;
            _box.XPositiveBulge = shapeData.XPositiveBulge;
            _box.XNegativeBulge = shapeData.XNegativeBulge;
            _box.YPositiveBulge = shapeData.YPositiveBulge;
            _box.YNegativeBulge = shapeData.YNegativeBulge;
            _box.ZPositiveBulge = shapeData.ZPositiveBulge;
            _box.ZNegativeBulge = shapeData.ZNegativeBulge;

            PhysicsMaterial = material;
        }

        public LaunchData Simulate()
        {
            var startPosition = Position;
            var startVelocity = Velocity;
            var startRotation = Rotation;
            var startAngularVelocity = AngularVelocity;
            var virtualPosition = Position;
            var virtualVelocity = Velocity;
            var virtualRotation = Rotation;
            var virtualAngularVelocity = AngularVelocity;
            var totalAirTime = 0f;
            var g = Gravity;
            var r = (Shape.Size.x + Shape.Size.y + Shape.Size.z) / 3 * 0.5f;
            var maxItterations = 100;
            var raycastSkippedTotalDistance = 0f;
            _state.Clear();
            for (int i = 0; i < maxItterations; i++)
            {
                _state.IterationCount++;
                var p = virtualPosition;
                var v = virtualVelocity;

                // Get step time.
                var stepped_t = 1f;
                if (v.sqrMagnitude > 0)
                {
                    // Get time at the point where the angle changes significantly.
                    var v2 = new Vector2(new Vector2(v.x, v.z).magnitude, v.y);
                    var rad = Mathf.Atan2(v2.y, v2.x);
                    var angle = rad * Mathf.Rad2Deg;
                    var stepped_angle = angle - StepAngle;
                    if (stepped_angle > -90 && stepped_angle < 90)
                    {
                        var stepped_rad = stepped_angle * Mathf.Deg2Rad;
                        var befCos = Mathf.Cos(rad);
                        var aftCos = Mathf.Cos(stepped_rad);
                        var stepped_vy = Mathf.Sin(stepped_rad) * (befCos / aftCos) * v.magnitude;
                        stepped_t = (stepped_vy - v.y) / g;
                        if (stepped_t <= 0f)
                        {
                            stepped_t = 1f;
                        }
                        stepped_t = Mathf.Clamp(stepped_t, 0f, 1f);
                    }
                }

                var stepped_p = p + v * stepped_t + Vector3.up * (0.5f * g * stepped_t * stepped_t);
                var stepped_v = v + Vector3.up * g * stepped_t;

                // Execute the raycast or proceed to the next step without doing anything.
                var toSteppedVec = stepped_p - p;
                var dist = toSteppedVec.magnitude;
                raycastSkippedTotalDistance += dist;
                var raycastRequired = raycastSkippedTotalDistance > ErrorDistance;
                if (raycastRequired)
                {
                    _state.RaycastPositionLog.Add(Position);
                    var rayVec = stepped_p - Position; // Start from the point where the last raycast was completed.
                    raycastSkippedTotalDistance = 0f;
                    _state.RaycastCount++;
                    if (Physics.SphereCast(Position, r - SpaceToWall, rayVec.normalized, out var hit, rayVec.magnitude, LayerMask))
                    {
                        var hit_p = hit.point + hit.normal * r;
                        var hit_diff_from_st = hit_p - startPosition;

                        // Sleep check
                        if (hit_diff_from_st.sqrMagnitude < SleepPositionThreshold * SleepPositionThreshold
                            && startVelocity.sqrMagnitude < SleepVelocityThreshold * SleepVelocityThreshold)
                        {
                            IsSleep = true;
                            return new LaunchData
                            {
                                Position = startPosition,
                                Velocity = Vector3.zero,
                                Rotation = startRotation,
                                AngularVelocity = Vector3.zero,
                                Gravity = g,
                                Duration = 0f,
                            };
                        }

                        // Calculate time to hit and velocity.
                        // * Recalculate the initial velocity to match the parabola to the hit point.
                        var hit_diff2_from_st = new Vector2(new Vector2(hit_diff_from_st.x, hit_diff_from_st.z).magnitude, hit_diff_from_st.y);
                        var st_v2 = new Vector2(new Vector2(startVelocity.x, startVelocity.z).magnitude, startVelocity.y);
                        var hit_t = 0f;
                        var v0y = 0f;
                        if (st_v2.x > 0)
                        {
                            hit_t = hit_diff2_from_st.x / st_v2.x;
                            v0y = (hit_diff2_from_st.y - 0.5f * g * hit_t * hit_t) / hit_t;
                        }
                        else
                        {
                            hit_t = AproximateT(st_v2.y, g, hit_diff2_from_st.y);
                            v0y = st_v2.y;
                        }
                        hit_t = Mathf.Clamp(hit_t, 0.0001f, 100f);
                        var v0 = new Vector3(startVelocity.x, v0y, startVelocity.z);



                        // Finalize the launch data.
                        var launchData = new LaunchData
                        {
                            Position = startPosition,
                            Velocity = v0,
                            Rotation = startRotation,
                            AngularVelocity = startAngularVelocity,
                            Gravity = g,
                            Duration = hit_t,
                        };

                        // Calculate the position and velocity after reflection.
                        Position = hit_p;
                        Rotation = ProjectileUtility.ApplyAngularVelocity(startRotation, startAngularVelocity, hit_t);
                        Velocity = launchData.Velocity + Vector3.up * g * hit_t;

                        _box.Position = Position;
                        _box.Rotation = Rotation;
                        var boxHitPosition = _box.GetClosestSurfaceWithPlane(hit.normal);
                        var boxHitNormal = (Position - boxHitPosition).normalized;
                        _state.IsHit = true;
                        _state.HitPosition = boxHitPosition;
                        _state.HitNormal = boxHitNormal;

                        if (Vector3.Dot(Velocity, boxHitNormal) < 0)
                        {
                            Velocity = Vector3.Reflect(Velocity, boxHitNormal);
                            if (PhysicsMaterial == null)
                            {
                                Velocity -= Vector3.Project(Velocity, boxHitNormal) * 0.5f;
                            }
                            else
                            {
                                var hitMat = hit.collider.sharedMaterial ?? PhysicsMaterial;
                                var frictionCombine = ProjectileUtility.MergePhysicsMaterialCombine(PhysicsMaterial.frictionCombine, hitMat.frictionCombine);
                                var friction = ProjectileUtility.CombineFriction(frictionCombine, PhysicsMaterial.dynamicFriction, hitMat.dynamicFriction);
                                friction *= 1 - Mathf.Clamp(Vector3.Distance(hit.point, boxHitPosition) / (r * 0.5f), 0, 1);
                                Velocity -= Vector3.ProjectOnPlane(Velocity, boxHitNormal) * friction;
                                var bounceCombine = ProjectileUtility.MergePhysicsMaterialCombine(PhysicsMaterial.bounceCombine, hitMat.bounceCombine);
                                var bounce = ProjectileUtility.CombineFriction(bounceCombine, PhysicsMaterial.bounciness, hitMat.bounciness);
                                Velocity -= Vector3.Project(Velocity, boxHitNormal) * (1 - bounce);
                            }
                        }

                        // Calculate angular velocity
                        var planeV = Vector3.ProjectOnPlane(Velocity, boxHitNormal);
                        var axis = Vector3.Cross(boxHitNormal, planeV.normalized);
                        AngularVelocity = axis * (planeV.magnitude / r);

                        return launchData;
                    }

                    Position = stepped_p;
                    Velocity = stepped_v;
                    Rotation = ProjectileUtility.ApplyAngularVelocity(virtualRotation, virtualAngularVelocity, stepped_t);
                    _box.Position = Position;
                    _box.Rotation = Rotation;
                }

                // Update the virtual transform.
                // The transform is not finalized until the raycast is complete.
                virtualPosition = stepped_p;
                virtualVelocity = stepped_v;
                totalAirTime += stepped_t;
                if (_state.RaycastCount >= MaxRaycastCount || totalAirTime >= MaxDuration)
                {
                    Position = virtualPosition;
                    Velocity = virtualVelocity;
                    Rotation = ProjectileUtility.ApplyAngularVelocity(virtualRotation, virtualAngularVelocity, totalAirTime);
                    _box.Position = Position;
                    _box.Rotation = Rotation;
                    break;
                }
            }

            return new LaunchData
            {
                Position = startPosition,
                Velocity = startVelocity,
                Rotation = startRotation,
                AngularVelocity = startAngularVelocity,
                Gravity = g,
                Duration = totalAirTime,
            };
        }

        private float AproximateT(float v0, float a, float d)
        {
            var v0sqr = v0 * v0;
            var ad2 = a * d * 2;
            var sqrt = Mathf.Sqrt(v0sqr + ad2);
            var t1 = (-v0 + sqrt) / a;
            var t2 = (-v0 - sqrt) / a;
            return Mathf.Max(t1, t2);
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