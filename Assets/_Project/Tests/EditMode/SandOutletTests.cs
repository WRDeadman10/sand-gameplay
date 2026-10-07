using NUnit.Framework;

using SandGameplay.Sand;
using SandGameplay.Transfer;

namespace SandGameplay.Tests {
    public sealed class SandOutletTests {
        private const uint Seed = 7u;
        private const int Yellow = 0;
        private const int Red = 1;

        [Test]
        public void Transfer_RemovesOnlyGrainsInsideSpan_AndMapsInteriorColumns() {
            SandGrid tank = CreateBottomRow(10, Yellow);
            GrainTransferQueue queue = new GrainTransferQueue(64);

            int moved = SandOutlet.Transfer(tank, new OutletSpan(3, 7, 3), ContainerColorRule.MatchColor, Yellow, 0, true, queue);

            Assert.AreEqual(4, moved);
            Assert.AreEqual(6, tank.GrainCount);
            Assert.IsFalse(tank.IsEmpty(2, 0));
            Assert.IsTrue(tank.IsEmpty(3, 0));
            Assert.IsTrue(tank.IsEmpty(6, 0));
            Assert.IsFalse(tank.IsEmpty(7, 0));
            Assert.IsTrue(queue.TryDequeue(out ushort cell, out int column));
            Assert.AreEqual(Yellow, SandCell.GetColorId(cell));
            Assert.AreEqual(0, column);
        }

        [Test]
        public void Transfer_MatchColor_LeavesOtherColorsInPlace() {
            SandGrid tank = CreateBottomRow(4, Yellow);
            tank.SetCell(1, 0, SandCell.CreateGrain(Red, 7));
            GrainTransferQueue queue = new GrainTransferQueue(16);

            int moved = SandOutlet.Transfer(tank, new OutletSpan(0, 4, 0), ContainerColorRule.MatchColor, Yellow, 0, true, queue);

            Assert.AreEqual(3, moved);
            Assert.AreEqual(Red, SandCell.GetColorId(tank.GetCell(1, 0)));
        }

        [Test]
        public void Transfer_AcceptAny_TakesEveryColor() {
            SandGrid tank = CreateBottomRow(4, Yellow);
            tank.SetCell(1, 0, SandCell.CreateGrain(Red, 7));
            GrainTransferQueue queue = new GrainTransferQueue(16);

            int moved = SandOutlet.Transfer(tank, new OutletSpan(0, 4, 0), ContainerColorRule.AcceptAny, Yellow, 0, true, queue);

            Assert.AreEqual(4, moved);
            Assert.AreEqual(0, tank.GrainCount);
        }

        [Test]
        public void Transfer_MaxGrains_LimitsOneBatch() {
            SandGrid tank = CreateBottomRow(8, Yellow);
            GrainTransferQueue queue = new GrainTransferQueue(16);

            int moved = SandOutlet.Transfer(tank, new OutletSpan(0, 8, 0), ContainerColorRule.MatchColor, Yellow, 3, false, queue);

            Assert.AreEqual(3, moved);
            Assert.AreEqual(5, tank.GrainCount);
            Assert.IsTrue(tank.IsEmpty(7, 0));
        }

        [Test]
        public void Transfer_FullQueue_StopsWithoutLosingGrains() {
            SandGrid tank = CreateBottomRow(6, Yellow);
            GrainTransferQueue queue = new GrainTransferQueue(2);

            int moved = SandOutlet.Transfer(tank, new OutletSpan(0, 6, 0), ContainerColorRule.MatchColor, Yellow, 0, true, queue);

            Assert.AreEqual(2, moved);
            Assert.AreEqual(4, tank.GrainCount);
            Assert.AreEqual(2, queue.Count);
        }

        [Test]
        public void Transfer_EmptySpan_DoesNothing() {
            SandGrid tank = CreateBottomRow(4, Yellow);
            GrainTransferQueue queue = new GrainTransferQueue(4);

            int moved = SandOutlet.Transfer(tank, default(OutletSpan), ContainerColorRule.MatchColor, Yellow, 0, true, queue);

            Assert.AreEqual(0, moved);
            Assert.AreEqual(4, tank.GrainCount);
        }

        [Test]
        public void CountAcceptable_FollowsRule() {
            SandGrid tank = CreateBottomRow(4, Yellow);
            tank.SetCell(0, 0, SandCell.CreateGrain(Red, 7));

            Assert.AreEqual(3, ContainerColorRule.MatchColor.CountAcceptable(tank, Yellow));
            Assert.AreEqual(4, ContainerColorRule.AcceptAny.CountAcceptable(tank, Yellow));
        }

        private static SandGrid CreateBottomRow(int width, int colorId) {
            SandGrid tank = new SandGrid(width, 2, Seed);
            for (int x = 0; x < width; x++) {
                tank.TrySpawn(x, 0, SandCell.CreateGrain(colorId, 7));
            }

            return tank;
        }
    }
}