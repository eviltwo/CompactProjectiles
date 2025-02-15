using UnityEngine;

namespace CompactProjectiles
{
    /// <summary>
    /// Each face of the box can bulge outwards in a different direction.
    /// 0.0 is box face, 1.0 is sphere.
    /// </summary>
    public class BulgingBox
    {
        public Vector3 Position = Vector3.zero;

        public Quaternion Rotation = Quaternion.identity;

        public Vector3 Scale = Vector3.one;

        private static float _halfBoxSize = 0.5f;

        public static float HalfBoxSize => _halfBoxSize;

        private static float _localSphereRadius = Mathf.Sqrt(3) * HalfBoxSize;

        /// <summary>
        /// Radius is center to corner of box.
        /// </summary>
        public static float LocalSphereRadius => _localSphereRadius;

        private readonly float[] _bulges = new float[6];

        public float XPositiveBulge
        {
            get => _bulges[0];
            set => _bulges[0] = value;
        }

        public float XNegativeBulge
        {
            get => _bulges[1];
            set => _bulges[1] = value;
        }

        public float YPositiveBulge
        {
            get => _bulges[2];
            set => _bulges[2] = value;
        }

        public float YNegativeBulge
        {
            get => _bulges[3];
            set => _bulges[3] = value;
        }

        public float ZPositiveBulge
        {
            get => _bulges[4];
            set => _bulges[4] = value;
        }

        public float ZNegativeBulge
        {
            get => _bulges[5];
            set => _bulges[5] = value;
        }

        private static readonly Vector3[] _faceDirections = new Vector3[6]
        {
            Vector3.right,
            Vector3.left,
            Vector3.up,
            Vector3.down,
            Vector3.forward,
            Vector3.back
        };

        public void SetBulge(float bulge)
        {
            for (var i = 0; i < 6; i++)
            {
                _bulges[i] = bulge;
            }
        }

        public void SetBulge(int faceIndex, float bulge)
        {
            _bulges[faceIndex] = bulge;
        }

        public Vector3 SphereToWorldPoint(Vector3 localPoint)
        {
            localPoint = ApplyBulge(localPoint);
            var matrix = Matrix4x4.TRS(Position, Rotation, Scale);
            var worldPoint = matrix.MultiplyPoint3x4(localPoint);
            return worldPoint;
        }

        public Vector3 WorldToSpherePoint(Vector3 worldPoint)
        {
            var matrix = Matrix4x4.TRS(Position, Rotation, Scale).inverse;
            var localPoint = matrix.MultiplyPoint3x4(worldPoint);
            localPoint = InvertBulge(localPoint);
            return localPoint;
        }

        public Vector3 SphereToWorldVector(Vector3 localVector)
        {
            localVector = InvertBulge(localVector);
            var matrix = Matrix4x4.TRS(Position, Rotation, Scale);
            var worldDirection = matrix.MultiplyVector(localVector);
            return worldDirection;
        }

        public Vector3 WorldToSphereVector(Vector3 worldVector)
        {
            var matrix = Matrix4x4.TRS(Position, Rotation, Scale).inverse;
            var localVector = matrix.MultiplyVector(worldVector);
            localVector = ApplyBulge(localVector);
            return localVector;
        }

        public Vector3 SphereToWorldDirection(Vector3 localDirection)
        {
            return SphereToWorldVector(localDirection).normalized;
        }

        public Vector3 WorldToSphereDirection(Vector3 worldDirection)
        {
            return WorldToSphereVector(worldDirection).normalized;
        }

        private Vector3 ApplyBulge(Vector3 localPoint)
        {
            for (var d = 0; d < 3; d++)
            {
                var v = Vector3.zero;
                v[d] = localPoint[d];
                var faceIndex = FindFaceIndex(v);
                var bulge = _bulges[faceIndex];
                var x = Mathf.Abs(localPoint[d]);
                var inner = Mathf.Min(HalfBoxSize, x);
                var outer = Mathf.Max(0, x - HalfBoxSize);
                outer *= bulge;
                localPoint[d] = Mathf.Sign(localPoint[d]) * (inner + outer);
            }

            return localPoint;
        }

        private Vector3 InvertBulge(Vector3 localPoint)
        {
            for (var d = 0; d < 3; d++)
            {
                var v = Vector3.zero;
                v[d] = localPoint[d];
                var faceIndex = FindFaceIndex(v);
                var bulge = _bulges[faceIndex];
                if (bulge == 0)
                {
                    continue;
                }
                var x = Mathf.Abs(localPoint[d]);
                var inner = Mathf.Min(HalfBoxSize, x);
                var outer = Mathf.Max(0, x - HalfBoxSize);
                outer = outer / bulge;
                localPoint[d] = Mathf.Sign(localPoint[d]) * (inner + outer);
            }

            return localPoint;
        }

        private int FindFaceIndex(Vector3 localDirection)
        {
            var maxDot = -1f;
            var faceIndex = 0;
            for (var i = 0; i < 6; i++)
            {
                var dot = Vector3.Dot(localDirection, _faceDirections[i]);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    faceIndex = i;
                }
            }
            return faceIndex;
        }

        public Vector3 GetClosestSurface(Vector3 position)
        {
            var dir = WorldToSphereDirection(position - Position);
            return SphereToWorldPoint(dir * LocalSphereRadius);
        }

        public Vector3 GetClosestSurfaceWithPlane(Vector3 normal)
        {
            var dir = WorldToSphereDirection(-normal);
            var boxSurfacePoint = SphereToWorldPoint(dir * LocalSphereRadius);

            var faceIndex = FindFaceIndex(dir);
            var bulge = _bulges[faceIndex];
            if (bulge == 1)
            {
                return boxSurfacePoint;
            }

            var cubeSurfacePoint = GetClosestCubeSurfaceWithPlane(normal);
            return Vector3.Lerp(cubeSurfacePoint, boxSurfacePoint, bulge);
        }

        private static Vector3[] CubeVertices = new Vector3[8]
        {
            new Vector3(-1, -1, -1),
            new Vector3(-1, -1, 1),
            new Vector3(-1, 1, -1),
            new Vector3(-1, 1, 1),
            new Vector3(1, -1, -1),
            new Vector3(1, -1, 1),
            new Vector3(1, 1, -1),
            new Vector3(1, 1, 1)
        };

        private static Vector3[] _cubeVertBuffer = new Vector3[8];
        private static float[] _cubeDistanceBuffer = new float[8];
        private static float _cubeVertexError = 0.1f;
        private Vector3 GetClosestCubeSurfaceWithPlane(Vector3 normal)
        {
            var matrix = Matrix4x4.TRS(Position, Rotation, Scale);

            for (var i = 0; i < 8; i++)
            {
                _cubeVertBuffer[i] = matrix.MultiplyPoint3x4(CubeVertices[i] * HalfBoxSize);
            }

            for (var i = 0; i < 8; i++)
            {
                _cubeDistanceBuffer[i] = Vector3.Dot(_cubeVertBuffer[i], normal);
            }

            var minDistance = float.MaxValue;
            for (var i = 0; i < 8; i++)
            {
                var distance = _cubeDistanceBuffer[i];
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }

            var closestPointTotal = Vector3.zero;
            var closestPointCount = 0;
            for (var i = 0; i < 8; i++)
            {
                if (Mathf.Abs(_cubeDistanceBuffer[i] - minDistance) < _cubeVertexError)
                {
                    closestPointTotal += _cubeVertBuffer[i];
                    closestPointCount++;
                }
            }

            return closestPointTotal / closestPointCount;
        }
    }
}
