using UnityEngine;

namespace CompactProjectiles
{
    /// <summary>
    /// Each face of the box can bulge outwards in a different direction.
    /// 0.0 is box face, 1.0 is sphere.
    /// </summary>
    public class BulgingBox
    {
        public Vector3 Position;

        public Quaternion Rotation;

        public Vector3 Scale = Vector3.one;

        public static float HalfBoxSize = 0.5f;

        /// <summary>
        /// Radius is center to corner of box.
        /// </summary>
        public static float LocalSphereRadius = Mathf.Sqrt(3) * HalfBoxSize;

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
            var matrix = Matrix4x4.TRS(Position, Rotation, Scale);
            var worldPoint = matrix.MultiplyPoint3x4(localPoint);
            return worldPoint;
        }
    }
}
