using UnityEngine;

namespace CompactProjectiles
{
    public class BulgingBoxDrawer : MonoBehaviour
    {
        public float BulgeAmount = 1.0f;

        public bool BulgeEachFace = false;

        public int LineCount = 8;

        private BulgingBox _bulgingBox;
        private BulgingBox BulgingBox
        {
            get
            {
                if (_bulgingBox == null)
                {
                    _bulgingBox = new BulgingBox();
                }

                _bulgingBox.Position = transform.position;
                _bulgingBox.Rotation = transform.rotation;
                _bulgingBox.Scale = transform.localScale;
                if (!BulgeEachFace)
                {
                    _bulgingBox.SetBulge(BulgeAmount);
                }

                return _bulgingBox;
            }
        }

        private void OnDrawGizmos()
        {
            var box = BulgingBox;

            for (int d = 0; d < 3; d++)
            {
                var dir = Vector3.zero;
                dir[d] = 1f;
                DrawFaceGizmos(box, d, dir, BulgingBox.LocalSphereRadius);
            }
        }

        private void DrawFaceGizmos(BulgingBox box, int dIndex, Vector3 localDirection, float radius)
        {
            var d1 = (dIndex + 1) % 3;
            var d2 = (dIndex + 2) % 3;
            for (int i = 0; i < LineCount; i++)
            {
                var ti = (float)(i + 1) / (LineCount + 1);
                var scale = Mathf.Cos(ti * Mathf.PI) * radius;
                var dir = localDirection * scale;
                var r = Mathf.Sin(ti * Mathf.PI) * radius;
                var sectionCount = (LineCount + 1) * 4;
                for (int j = 0; j < sectionCount; j++)
                {
                    var tj = (float)j / sectionCount;
                    var rad = tj * Mathf.PI * 2;
                    var p1 = dir;
                    p1[d1] = Mathf.Cos(rad) * r;
                    p1[d2] = Mathf.Sin(rad) * r;
                    tj = (float)(j + 1) / sectionCount;
                    rad = tj * Mathf.PI * 2;
                    var p2 = dir;
                    p2[d1] = Mathf.Cos(rad) * r;
                    p2[d2] = Mathf.Sin(rad) * r;
                    Gizmos.DrawLine(box.SphereToWorld(p1), box.SphereToWorld(p2));
                }
            }
        }
    }
}
