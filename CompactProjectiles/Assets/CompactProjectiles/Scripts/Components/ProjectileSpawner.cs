using UnityEngine;

namespace CompactProjectiles
{
    public class ProjectileSpawner : MonoBehaviour
    {
        public float SpawnRate = 1f;

        public PhysicsMaterial PhysicsMaterial;

        public float MaxVelocity = 5f;

        public float MaxAngularVelocity = 2f;

        private float _spawnElapsedTime;

        private void Update()
        {
            _spawnElapsedTime += Time.deltaTime;
            if (_spawnElapsedTime >= 1 / SpawnRate)
            {
                _spawnElapsedTime = 0;
                Spawn(transform.position);
            }
        }

        public void Spawn(Vector3 position)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (cube.TryGetComponent<Collider>(out var collider))
            {
                Destroy(collider);
            }
            cube.transform.position = position;
            cube.transform.rotation = Random.rotation;
            cube.transform.localScale = new Vector3(
                Random.Range(0.5f, 2.0f),
                Random.Range(0.5f, 2.0f),
                Random.Range(0.5f, 2.0f));
            var projectile = cube.AddComponent<ProjectileController>();
            projectile.PhysicsMaterial = PhysicsMaterial;
            projectile.ShapeData = new BoxProjectile.ShapeData();
            projectile.ShapeData.XNegativeBulge = Random.Range(0f, 1f);
            projectile.ShapeData.XPositiveBulge = Random.Range(0f, 1f);
            projectile.ShapeData.YNegativeBulge = Random.Range(0f, 1f);
            projectile.ShapeData.YPositiveBulge = Random.Range(0f, 1f);
            projectile.ShapeData.ZPositiveBulge = Random.Range(0f, 1f);
            projectile.ShapeData.ZNegativeBulge = Random.Range(0f, 1f);
            projectile.StartVelocity = Random.insideUnitSphere * MaxVelocity;
            projectile.StartAngularVelocity = Random.insideUnitSphere * MaxAngularVelocity;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }
}
