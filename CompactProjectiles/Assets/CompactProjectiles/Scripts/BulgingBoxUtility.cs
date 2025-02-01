using UnityEngine;

namespace CompactProjectiles
{
    public static class BulgingBoxUtility
    {
        public static void DrawBoxGizmos(BulgingBox box, int lineCount)
        {
            for (int d = 0; d < 3; d++)
            {
                var dir = Vector3.zero;
                dir[d] = 1f;
                DrawFaceGizmos(box, d, dir, BulgingBox.LocalSphereRadius, lineCount);
            }
        }

        private static void DrawFaceGizmos(BulgingBox box, int dIndex, Vector3 localDirection, float radius, float lineCount)
        {
            var d1 = (dIndex + 1) % 3;
            var d2 = (dIndex + 2) % 3;
            for (int i = 0; i < lineCount; i++)
            {
                var ti = (float)(i + 1) / (lineCount + 1);
                var scale = Mathf.Cos(ti * Mathf.PI) * radius;
                var dir = localDirection * scale;
                var r = Mathf.Sin(ti * Mathf.PI) * radius;
                var sectionCount = (lineCount + 1) * 4;
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
                    Gizmos.DrawLine(box.SphereToWorldPoint(p1), box.SphereToWorldPoint(p2));
                }
            }
        }
    }
}
