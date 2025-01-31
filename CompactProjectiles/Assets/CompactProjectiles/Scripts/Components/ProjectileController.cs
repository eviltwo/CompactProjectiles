using UnityEngine;

namespace CompactProjectiles
{
    public class ProjectileController : MonoBehaviour
    {
        public PhysicsMaterial PhysicsMaterial;

        public float StepAngle = 45f;

        public float ErrorDistance = 0.5f;

        public float MaxDuration = 5f;

        public int MaxRaycastCount = 5;

        public Vector3 Velocity = Vector3.right;

        private BoxProjectile _projectile;

        public LayerMask LayerMask = -1;

        public int SimulationCount = 1;

        public bool DrawInfo;

        public bool DrawDummyLine;

        private void Awake()
        {
            _projectile = new BoxProjectile(PhysicsMaterial);
            _projectile.Position = transform.position;
            _projectile.Velocity = Velocity;
        }

        private void Update()
        {
            _projectile.StepAngle = StepAngle;
            _projectile.ErrorDistance = ErrorDistance;
            _projectile.MaxDuration = MaxDuration;
            _projectile.MaxRaycastCount = MaxRaycastCount;
            _projectile.LayerMask = LayerMask;

            _projectile.Simulate();
        }

        private void OnDrawGizmos()
        {
            var projectile = new BoxProjectile(PhysicsMaterial);
            projectile.Position = transform.position;
            projectile.Velocity = Velocity;
            projectile.StepAngle = StepAngle;
            projectile.ErrorDistance = ErrorDistance;
            projectile.MaxDuration = MaxDuration;
            projectile.MaxRaycastCount = MaxRaycastCount;
            projectile.LayerMask = LayerMask;
            for (int i = 0; i < SimulationCount; i++)
            {
                var customData = new LaunchData
                {
                    Position = projectile.Position,
                    Velocity = projectile.Velocity,
                    Gravity = projectile.Gravity,
                };

                var data = projectile.Simulate();

                var drawStep = 0.01f;
                Gizmos.color = Color.red;
                var lastPos = data.Position;
                for (var t = 0f; t < data.Duration; t += drawStep)
                {
                    ProjectileUtility.LaunchSimulation(data, t, out var p);
                    Gizmos.DrawLine(lastPos, p);
                    lastPos = p;
                }
                Gizmos.color = new Color(1, 0, 0, 0.2f);
                Gizmos.DrawCube(lastPos, Vector3.one);

#if UNITY_EDITOR
                if (DrawInfo)
                {
                    var oldColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0, 0, 0, 0.5f);
                    var guiStyle = new GUIStyle(GUI.skin.label);
                    guiStyle.alignment = TextAnchor.LowerLeft;
                    guiStyle.normal.background = Texture2D.whiteTexture;
                    var labelPos = data.IsHit ? data.HitPoint : data.Position;
                    UnityEditor.Handles.Label(labelPos + Vector3.up, $"t: {data.Duration}\nitr:{data.IterationCount}\nraycast: {data.RaycastCount}", guiStyle);
                    GUI.backgroundColor = oldColor;
                }
#endif

                if (DrawDummyLine)
                {
                    Gizmos.color = Color.green;
                    customData.Duration = 10;
                    lastPos = customData.Position;
                    for (var t = 0f; t < customData.Duration; t += drawStep)
                    {
                        ProjectileUtility.LaunchSimulation(customData, t, out var p);
                        Gizmos.DrawLine(lastPos, p);
                        lastPos = p;
                    }
                }
            }
        }
    }
}
