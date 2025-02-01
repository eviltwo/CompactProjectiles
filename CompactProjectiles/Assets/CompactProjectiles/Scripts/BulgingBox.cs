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

        public static float HalfBoxSize => 0.5f;

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

        public Vector3 SphereToWorld(Vector3 localPoint)
        {
            var faceIndex = FindFaceIndex(localPoint);
            var bulge = _bulges[faceIndex];
            for (var d = 0; d < 3; d++)
            {
                var inner = Mathf.Min(HalfBoxSize, Mathf.Abs(localPoint[d]));
                var outer = Mathf.Max(0, Mathf.Abs(localPoint[d]) - HalfBoxSize);
                outer *= bulge;
                localPoint[d] = Mathf.Sign(localPoint[d]) * (inner + outer);
            }

            var matrix = Matrix4x4.TRS(Position, Rotation, Scale);
            var worldPoint = matrix.MultiplyPoint3x4(localPoint);
            return worldPoint;
        }

        public Vector3 WorldToSphere(Vector3 worldPoint)
        {
            var matrix = Matrix4x4.TRS(Position, Rotation, Scale).inverse;
            var localPoint = matrix.MultiplyPoint3x4(worldPoint);

            var faceIndex = FindFaceIndex(localPoint);
            var bulge = _bulges[faceIndex];
            if (bulge != 0)
            {
                for (var d = 0; d < 3; d++)
                {
                    var inner = Mathf.Min(HalfBoxSize, Mathf.Abs(localPoint[d]));
                    var outer = Mathf.Max(0, Mathf.Abs(localPoint[d]) - HalfBoxSize);
                    outer *= 1 / bulge;
                    localPoint[d] = Mathf.Sign(localPoint[d]) * (inner + outer);
                }
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
    }
}
