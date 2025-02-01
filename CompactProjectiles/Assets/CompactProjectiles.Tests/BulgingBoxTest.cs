using NUnit.Framework;
using UnityEngine;

namespace CompactProjectiles.Tests
{
    public class BulgingBoxTest
    {
        private BulgingBox _box;

        [SetUp]
        public void SetUp()
        {
            _box = new BulgingBox();
        }

        [TestCase(1, 0, 0, 0.5f)]
        [TestCase(0, 1, 0, 0.5f)]
        [TestCase(0, 0, 1, 0.5f)]
        [TestCase(1, 0, 0, 1)]
        [TestCase(0, 1, 0, 1)]
        [TestCase(0, 0, 1, 1)]
        [TestCase(0, 0, 0, 0.5f)]
        [TestCase(0.5f, 0, 0, 0.5f)]
        [TestCase(2, 0, 0, 0.5f)]
        [TestCase(1, 1, 1, 0.5f)]
        [TestCase(0.5f, 0, 0, 0)]
        public void CollectTransformAndInvert(float x, float y, float z, float bulge)
        {
            _box.SetBulge(bulge);
            var local = _box.WorldToSphere(new Vector3(x, y, z));
            var world = _box.SphereToWorld(local);
            Assert.AreEqual(x, world.x);
            Assert.AreEqual(y, world.y);
            Assert.AreEqual(z, world.z);
        }
    }
}
