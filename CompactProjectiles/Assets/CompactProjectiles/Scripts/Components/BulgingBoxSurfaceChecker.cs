using UnityEngine;

namespace CompactProjectiles
{
    public class BulgingBoxSurfaceChecker : MonoBehaviour
    {
        public BulgingBoxDrawer Drawer;

        private BulgingBoxDrawer GetDrawer()
        {
            if (Drawer == null)
            {
                Drawer = FindAnyObjectByType<BulgingBoxDrawer>();
            }
            return Drawer;
        }

        private void OnDrawGizmos()
        {
            var drawer = GetDrawer();
            if (drawer == null)
            {
                return;
            }

            var box = drawer.BulgingBox;

            // Draw myself
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(transform.position, 0.1f);

            // Draw point by center
            {
                var point = box.GetClosestSurface(transform.position);
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawSphere(point, 0.01f);
                Gizmos.DrawLine(transform.position, point);
            }

            // Draw point by plane
            {
                var point = box.GetClosestSurfaceWithPlane(transform.up);
                Gizmos.color = Color.green;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(1, 0, 1));
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.DrawSphere(point, 0.01f);
                Gizmos.DrawLine(point + Vector3.Project(transform.position - point, transform.up), point);
            }
        }
    }
}
