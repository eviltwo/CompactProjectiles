using System.Text;
using UnityEngine;

namespace CompactProjectiles
{
    public class ProjectileDebugger : MonoBehaviour
    {
        public BoxProjectile.ShapeData ShapeData;

        public PhysicsMaterial PhysicsMaterial;

        public float StepAngle = 45f;

        public float ErrorDistance = 0.5f;

        public float MaxDuration = 5f;

        public int MaxRaycastCount = 5;

        public float VelocityUp = 5f;

        public float VelocityForward = 5f;

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

        private static StringBuilder _infoBuilder = new StringBuilder();
        private void SimulateAndDrawGizmos(Vector3 velocity)
        {
            var projectile = new BoxProjectile(ShapeData, PhysicsMaterial);
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
                    Rotation = projectile.Rotation,
                    Gravity = projectile.Gravity,
                };

                var data = projectile.Simulate();
                var state = projectile.LastSimulationState;
                if (projectile.IsSleep)
                {
                    break;
                }

                var lineDrawStep = 0.01f;
                var lastPos = data.Position;
                Gizmos.color = Color.red;
                for (var t = 0f; t < data.Duration && t < 10; t += lineDrawStep)
                {
                    ProjectileUtility.LaunchSimulation(data, t, out var p);
                    Gizmos.DrawLine(lastPos, p);
                    lastPos = p;
                }

                foreach (var pos in state.RaycastPositionLog)
                {
                    Gizmos.DrawSphere(pos, 0.1f);
                }

                var cubeDrawStep = 0.2f;
                Gizmos.color = new Color(1, 0, 0, 0.25f);
                for (var t = 0f; t < data.Duration && t < 10; t += cubeDrawStep)
                {
                    ProjectileUtility.LaunchSimulation(data, t, out var p);
                    var rot = ProjectileUtility.ApplyAngularVelocity(data.Rotation, data.AngularVelocity, t);
                    Gizmos.matrix = Matrix4x4.TRS(p, rot, Vector3.one);
                    Gizmos.DrawWireCube(Vector3.zero, ShapeData.Size);
                }
                Gizmos.matrix = Matrix4x4.identity;

                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.matrix = Matrix4x4.TRS(projectile.Position, projectile.Rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                Gizmos.matrix = Matrix4x4.identity;

#if UNITY_EDITOR
                if (DrawInfo && i == SimulationCount - 1)
                {
                    var oldColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0, 0, 0, 0.8f);
                    var guiStyle = new GUIStyle(GUI.skin.label);
                    guiStyle.alignment = TextAnchor.LowerLeft;
                    guiStyle.normal.background = Texture2D.whiteTexture;
                    var labelPos = state.IsHit ? state.HitPosition : data.Position;
                    _infoBuilder.Clear();
                    _infoBuilder.Append($"t: {data.Duration},");
                    _infoBuilder.Append($"itr: {state.IterationCount},");
                    _infoBuilder.AppendLine($"ray: {state.RaycastCount}");
                    _infoBuilder.AppendLine($"p: {projectile.Position},");
                    _infoBuilder.AppendLine($"v: {projectile.Velocity}");
                    _infoBuilder.AppendLine($"r: {projectile.Rotation},");
                    _infoBuilder.AppendLine($"a: {projectile.AngularVelocity}");
                    _infoBuilder.Append($"hit: {state.IsHit},");
                    _infoBuilder.Append($"hitN: {state.HitNormal}");
                    UnityEditor.Handles.Label(labelPos + Vector3.up, _infoBuilder.ToString(), guiStyle);
                    GUI.backgroundColor = oldColor;
                }
#endif

                if (DrawDummyLine)
                {
                    Gizmos.color = Color.green;
                    customData.Duration = 10;
                    lastPos = customData.Position;
                    for (var t = 0f; t < customData.Duration; t += lineDrawStep)
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
