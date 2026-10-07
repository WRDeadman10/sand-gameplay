using NUnit.Framework;

using SandGameplay.Container;
using SandGameplay.Sand;

using UnityEngine;

namespace SandGameplay.Tests {
    public sealed class InteriorSpawnerTests {
        private SandPalette _palette;

        [SetUp]
        public void SetUp() {
            _palette = ScriptableObject.CreateInstance<SandPalette>();
        }

        [TearDown]
        public void TearDown() {
            Object.DestroyImmediate(_palette);
        }

        [Test]
        public void Spawn_HalfRatio_SpawnsOneVisualGrainPerTwoLogicalGrains() {
            SandGrid interior = new SandGrid(10, 12, 3u);
            InteriorSpawner spawner = new InteriorSpawner(interior, 10, 0, 5u);
            spawner.Reset(200);

            for (int i = 0; i < 10; i++) {
                spawner.Spawn(SandCell.CreateGrain(0, 7), i);
            }

            Assert.AreEqual(100, spawner.VisualCapacity);
            Assert.AreEqual(5, interior.GrainCount);
        }

        [Test]
        public void Spawn_KeepsGrainColorAndTone() {
            SandGrid interior = new SandGrid(4, 5, 3u);
            InteriorSpawner spawner = new InteriorSpawner(interior, 4, 0, 5u);
            spawner.Reset(16);
            ushort grain = SandCell.CreateGrain(3, 12);

            spawner.Spawn(grain, 2);

            Assert.AreEqual(grain, interior.GetCell(2, interior.Height - 1));
        }

        [Test]
        public void FillToTop_FillsInterior_AndClearsHeadroom() {
            SandGrid interior = new SandGrid(6, 8, 3u);
            InteriorSpawner spawner = new InteriorSpawner(interior, 6, 0, 5u);
            interior.TrySpawn(0, 7, SandCell.CreateGrain(0, 7));

            spawner.FillToTop(_palette, 0, 7);

            Assert.AreEqual(36, interior.GrainCount);
            for (int x = 0; x < interior.Width; x++) {
                Assert.IsTrue(interior.IsEmpty(x, 6));
                Assert.IsTrue(interior.IsEmpty(x, 7));
                Assert.IsFalse(interior.IsEmpty(x, 0));
                Assert.IsFalse(interior.IsEmpty(x, 5));
            }
        }
    }
}