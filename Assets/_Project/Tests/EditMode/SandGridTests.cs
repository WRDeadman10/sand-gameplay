using System;

using NUnit.Framework;

using SandGameplay.Sand;

namespace SandGameplay.Tests {
    public sealed class SandGridTests {
        private const uint Seed = 12345u;
        private static readonly ushort Grain = SandCell.CreateGrain(0, 7);

        [Test]
        public void Step_GrainAboveEmptyCell_FallsOneCell() {
            SandGrid grid = new SandGrid(3, 5, Seed);
            grid.TrySpawn(1, 4, Grain);

            grid.Step(1f);

            Assert.IsTrue(grid.IsEmpty(1, 4));
            Assert.AreEqual(Grain, grid.GetCell(1, 3));
        }

        [Test]
        public void Step_GrainOnFloor_StaysAndGridSettles() {
            SandGrid grid = new SandGrid(3, 3, Seed);
            grid.TrySpawn(1, 0, Grain);

            grid.Step(1f);

            Assert.AreEqual(Grain, grid.GetCell(1, 0));
            Assert.IsTrue(grid.IsSettled);
        }

        [Test]
        public void Step_GrainResting_OnGrain_SlidesToOneDiagonal() {
            SandGrid grid = new SandGrid(3, 3, Seed);
            grid.TrySpawn(1, 0, Grain);
            grid.TrySpawn(1, 1, Grain);

            grid.Step(1f);

            Assert.IsTrue(grid.IsEmpty(1, 1));
            bool slidLeft = !grid.IsEmpty(0, 0);
            bool slidRight = !grid.IsEmpty(2, 0);
            Assert.IsTrue(slidLeft ^ slidRight);
        }

        [Test]
        public void Step_GrainAgainstWall_SlidesAwayFromWall() {
            SandGrid grid = new SandGrid(2, 2, Seed);
            grid.TrySpawn(0, 0, Grain);
            grid.TrySpawn(0, 1, Grain);

            grid.Step(1f);

            Assert.AreEqual(2, grid.GrainCount);
            Assert.IsFalse(grid.IsEmpty(1, 0));
            Assert.IsTrue(grid.IsEmpty(0, 1));
        }

        [Test]
        public void Step_ThousandTicks_ConservesGrainsAndSettles() {
            SandGrid grid = CreateNoisyGrid(40, 30, 0.6f);
            int initial = grid.GrainCount;

            for (int i = 0; i < 1000; i++) {
                grid.Step(1f);
            }

            Assert.AreEqual(initial, grid.GrainCount);
            Assert.AreEqual(initial, CountOccupied(grid));
            Assert.IsTrue(grid.IsSettled);
        }

        [Test]
        public void Step_SameSeedAndInput_ProducesIdenticalState() {
            SandGrid first = CreateNoisyGrid(24, 24, 0.5f);
            SandGrid second = CreateNoisyGrid(24, 24, 0.5f);

            for (int i = 0; i < 200; i++) {
                first.Step(0.8f);
                second.Step(0.8f);
            }

            Assert.IsTrue(first.Cells.SequenceEqual(second.Cells));
        }

        [Test]
        public void Step_SettledGrid_DoesNotChangeVersion() {
            SandGrid grid = new SandGrid(4, 4, Seed);
            grid.TrySpawn(1, 0, Grain);
            grid.Step(1f);
            int version = grid.Version;

            grid.Step(1f);

            Assert.IsTrue(grid.IsSettled);
            Assert.AreEqual(version, grid.Version);
        }

        [Test]
        public void Remove_SupportingGrain_WakesGridAndGrainAboveFalls() {
            SandGrid grid = new SandGrid(1, 3, Seed);
            grid.TrySpawn(0, 0, Grain);
            grid.TrySpawn(0, 1, Grain);
            grid.Step(1f);
            Assert.IsTrue(grid.IsSettled);

            ushort removed = grid.Remove(0, 0);
            grid.Step(1f);

            Assert.AreEqual(Grain, removed);
            Assert.AreEqual(Grain, grid.GetCell(0, 0));
            Assert.IsTrue(grid.IsEmpty(0, 1));
        }

        [Test]
        public void Step_LowSlideChance_KeepsStepping_UntilGrainSlides() {
            SandGrid grid = new SandGrid(3, 3, Seed);
            grid.TrySpawn(1, 0, Grain);
            grid.TrySpawn(1, 1, Grain);

            for (int i = 0; i < 500 && !grid.IsSettled; i++) {
                grid.Step(0.1f);
            }

            Assert.IsTrue(grid.IsSettled);
            Assert.IsTrue(grid.IsEmpty(1, 1));
        }

        [Test]
        public void TrySpawn_OccupiedOutsideOrEmptyValue_ReturnsFalse() {
            SandGrid grid = new SandGrid(2, 2, Seed);
            grid.TrySpawn(0, 0, Grain);

            Assert.IsFalse(grid.TrySpawn(0, 0, Grain));
            Assert.IsFalse(grid.TrySpawn(5, 0, Grain));
            Assert.IsFalse(grid.TrySpawn(1, 1, SandCell.Empty));
            Assert.AreEqual(1, grid.GrainCount);
        }

        [Test]
        public void SetCell_TracksGrainCount() {
            SandGrid grid = new SandGrid(2, 2, Seed);

            grid.SetCell(0, 0, Grain);
            grid.SetCell(0, 0, Grain);
            grid.SetCell(1, 1, Grain);
            grid.SetCell(1, 1, SandCell.Empty);

            Assert.AreEqual(1, grid.GrainCount);
        }

        [Test]
        public void CountColor_CountsOnlyThatColor() {
            SandGrid grid = new SandGrid(3, 1, Seed);
            grid.TrySpawn(0, 0, SandCell.CreateGrain(2, 3));
            grid.TrySpawn(1, 0, SandCell.CreateGrain(2, 9));
            grid.TrySpawn(2, 0, SandCell.CreateGrain(4, 3));

            Assert.AreEqual(2, grid.CountColor(2));
            Assert.AreEqual(1, grid.CountColor(4));
        }

        [Test]
        public void Step_AfterWarmUp_AllocatesNoManagedMemory() {
            SandGrid grid = CreateNoisyGrid(64, 64, 0.5f);
            grid.Step(0.8f);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 50; i++) {
                grid.Step(0.8f);
            }

            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.AreEqual(0L, after - before);
        }

        [Test]
        public void Constructor_NonPositiveSize_Throws() {
            Assert.Throws<ArgumentOutOfRangeException>(CreateZeroWidthGrid);
        }

        private static void CreateZeroWidthGrid() {
            SandGrid grid = new SandGrid(0, 4, Seed);
            Assert.IsNull(grid);
        }

        private static SandGrid CreateNoisyGrid(int width, int height, float density) {
            SandGrid grid = new SandGrid(width, height, Seed);
            XorShift32 random = new XorShift32(99u);
            for (int y = height / 3; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (random.NextFloat01() < density) {
                        grid.TrySpawn(x, y, SandCell.CreateGrain(random.NextInt(0, 4), random.NextInt(0, 15)));
                    }
                }
            }

            return grid;
        }

        private static int CountOccupied(SandGrid grid) {
            int count = 0;
            ReadOnlySpan<ushort> cells = grid.Cells;
            for (int i = 0; i < cells.Length; i++) {
                if (SandCell.IsGrain(cells[i])) {
                    count++;
                }
            }

            return count;
        }
    }
}