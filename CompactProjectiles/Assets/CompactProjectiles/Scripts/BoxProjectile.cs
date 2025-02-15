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
        public float AngularDrag;
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

        public float AngularDrag = 0.05f;

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
                var stepped_r = ProjectileUtility.TraceLaunchedRotation(virtualRotation, virtualAngularVelocity, AngularDrag, stepped_t);

                // Execute the raycast or proceed to the next step without doing anything.
                var toSteppedVec = stepped_p - p;
                var dist = toSteppedVec.magnitude;
                raycastSkippedTotalDistance += dist;
                var raycastRequired = raycastSkippedTotalDistance > ErrorDistance;
                if (raycastRequired)
                {
                    raycastSkippedTotalDistance = 0f;
                    _state.RaycastCount++;
                    _state.RaycastPositionLog.Add(Position);
                    var rayVec = stepped_p - Position; // Start from the point where the last raycast was completed.
                    var ray = new Ray(Position, rayVec.normalized);
                    if (Physics.SphereCast(ray, r - SpaceToWall, out var hit, rayVec.magnitude, LayerMask))
                    {
                        var hit_p = hit.point + hit.normal * r;
                        var hit_diff_from_st = hit_p - startPosition;

                        var hit_diff2_from_st = new Vector2(new Vector2(hit_diff_from_st.x, hit_diff_from_st.z).magnitude, hit_diff_from_st.y);
                        var st_v2 = new Vector2(new Vector2(startVelocity.x, startVelocity.z).magnitude, startVelocity.y);

                        // Sleep if the projectile is not moving.
                        if (hit_diff_from_st.sqrMagnitude < SleepPositionThreshold * SleepPositionThreshold
                            && startVelocity.sqrMagnitude < SleepVelocityThreshold * SleepVelocityThreshold)
                        {
                            IsSleep = true;

                        }

                        // Sleep if speed xz is too slow. because difficult to calculate the time to hit.
                        if (st_v2.x < SleepVelocityThreshold && Mathf.Abs(hit_diff_from_st.y) < SleepPositionThreshold)
                        {
                            IsSleep = true;
                        }

                        if (IsSleep)
                        {
                            return new LaunchData
                            {
                                Position = startPosition,
                                Velocity = Vector3.zero,
                                Rotation = startRotation,
                                AngularVelocity = Vector3.zero,
                                AngularDrag = AngularDrag,
                                Gravity = g,
                                Duration = 0f,
                            };
                        }

                        // Calculate time to hit and velocity.
                        // * Recalculate the initial velocity to match the parabola to the hit point.
                        var hit_t = 0f;
                        var v0y = 0f;
                        if (st_v2.x > 0 && hit_diff2_from_st.x > 0)
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
                            AngularDrag = AngularDrag,
                            Gravity = g,
                            Duration = hit_t,
                        };

                        // Calculate the position and velocity after reflection.
                        Position = hit_p;
                        Rotation = ProjectileUtility.TraceLaunchedRotation(startRotation, startAngularVelocity, AngularDrag, hit_t);
                        Velocity = launchData.Velocity + Vector3.up * g * hit_t;
                        var hitVelocity = Velocity;

                        _box.Position = Position;
                        _box.Rotation = Rotation;
                        var boxHitPosition = _box.GetClosestSurfaceWithPlane(hit.normal);
                        var boxHitNormal = (Position - boxHitPosition).normalized;
                        _state.IsHit = true;
                        _state.HitPosition = boxHitPosition;
                        _state.HitNormal = boxHitNormal;

                        if (Vector3.Dot(Velocity, boxHitNormal) > 0)
                        {
                            boxHitPosition = hit.point;
                            boxHitNormal = hit.normal;
                        }

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

                        // Calculate angular velocity
                        var hitDist = Vector3.Distance(Position, boxHitPosition);
                        AngularVelocity = Vector3.Cross(boxHitNormal, Velocity) / hitDist;

                        return launchData;
                    }

                    Position = stepped_p;
                    Velocity = stepped_v;
                    Rotation = stepped_r;
                    _box.Position = Position;
                    _box.Rotation = Rotation;
                }

                // Update the virtual transform.
                // The transform is not finalized until the raycast is complete.
                virtualPosition = stepped_p;
                virtualVelocity = stepped_v;
                virtualRotation = stepped_r;
                totalAirTime += stepped_t;
                if (_state.RaycastCount >= MaxRaycastCount || totalAirTime >= MaxDuration)
                {
                    Position = virtualPosition;
                    Velocity = virtualVelocity;
                    Rotation = ProjectileUtility.TraceLaunchedRotation(virtualRotation, virtualAngularVelocity, AngularDrag, totalAirTime);
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
                AngularDrag = AngularDrag,
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
}