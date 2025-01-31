using UnityEngine;

namespace CompactProjectiles
{
    public class ProjectileDebugger : MonoBehaviour
    {
        public PhysicsMaterial PhysicsMaterial;

        public float StepAngle = 45f;

        public float ErrorDistance = 0.5f;

        public float MaxDuration = 5f;

        public int MaxRaycastCount = 5;

        public float VelocityUp = 5f;

        public float VelocityForward = 5f;

        private BoxProjectile _projectile;

        public LayerMask LayerMask = -1;

        public int SimulationCount = 1;

        public int LineCount = 8;

        public bool DrawInfo;

        public bool DrawDummyLine;

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                return;
            }

            for (int i = 0; i < LineCount; i++)
            {
                var angle = i * 360f / LineCount;
                var forward = Quaternion.Euler(0, angle, 0) * Vector3.forward * VelocityForward;
                var up = Vector3.up * VelocityUp;
                SimulateAndDrawGizmos(forward + up);
            }
        }

        private void SimulateAndDrawGizmos(Vector3 velocity)
        {
            var projectile = new BoxProjectile(PhysicsMaterial);
            projectile.Position = transform.position;
            projectile.Velocity = velocity;
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
                if (data.IsSleep)
                {
                    break;
                }

                for (var t = 0f; t < data.Duration && t < 10; t += drawStep)
                {
                    ProjectileUtility.LaunchSimulation(data, t, out var p);
                    Gizmos.DrawLine(lastPos, p);
                    lastPos = p;
                }

                var state = projectile.LastSimulationState;
                foreach (var pos in state.RaycastPositionLog)
                {
                    Gizmos.DrawSphere(pos, 0.1f);
                }

                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawWireCube(lastPos, Vector3.one);

#if UNITY_EDITOR
                if (DrawInfo)
                {
                    var oldColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0, 0, 0, 0.5f);
                    var guiStyle = new GUIStyle(GUI.skin.label);
                    guiStyle.alignment = TextAnchor.LowerLeft;
                    guiStyle.normal.background = Texture2D.whiteTexture;
                    var labelPos = data.IsHit ? data.HitPoint : data.Position;
                    UnityEditor.Handles.Label(labelPos + Vector3.up, $"t: {data.Duration}\nitr:{state.IterationCount}\nraycast: {state.RaycastCount}", guiStyle);
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
