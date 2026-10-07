using NUnit.Framework;

using SandGameplay.Sand;
using SandGameplay.Transfer;

namespace SandGameplay.Tests {
    public sealed class GrainTransferQueueTests {
        [Test]
        public void Queue_IsFirstInFirstOut_AcrossWrapAround() {
            GrainTransferQueue queue = new GrainTransferQueue(3);
            queue.TryEnqueue(SandCell.CreateGrain(1, 1), 10);
            queue.TryEnqueue(SandCell.CreateGrain(2, 2), 20);
            queue.TryDequeue(out ushort ignoredCell, out int ignoredColumn);
            queue.TryEnqueue(SandCell.CreateGrain(3, 3), 30);
            queue.TryEnqueue(SandCell.CreateGrain(4, 4), 40);

            Assert.AreEqual(1, SandCell.GetColorId(ignoredCell));
            Assert.AreEqual(10, ignoredColumn);
            Assert.IsTrue(queue.TryDequeue(out ushort second, out int secondColumn));
            Assert.IsTrue(queue.TryDequeue(out ushort third, out int thirdColumn));
            Assert.IsTrue(queue.TryDequeue(out ushort fourth, out int fourthColumn));
            Assert.AreEqual(2, SandCell.GetColorId(second));
            Assert.AreEqual(20, secondColumn);
            Assert.AreEqual(3, SandCell.GetColorId(third));
            Assert.AreEqual(30, thirdColumn);
            Assert.AreEqual(4, SandCell.GetColorId(fourth));
            Assert.AreEqual(40, fourthColumn);
        }

        [Test]
        public void TryEnqueue_WhenFull_ReturnsFalse() {
            GrainTransferQueue queue = new GrainTransferQueue(1);

            Assert.IsTrue(queue.TryEnqueue(SandCell.CreateGrain(0, 0), 0));
            Assert.IsFalse(queue.TryEnqueue(SandCell.CreateGrain(0, 0), 0));
            Assert.AreEqual(1, queue.Count);
        }

        [Test]
        public void TryDequeue_WhenEmpty_ReturnsFalseAndEmptyCell() {
            GrainTransferQueue queue = new GrainTransferQueue(2);

            Assert.IsFalse(queue.TryDequeue(out ushort cell, out int column));
            Assert.AreEqual(SandCell.Empty, cell);
            Assert.AreEqual(0, column);
        }

        [Test]
        public void Clear_EmptiesQueue() {
            GrainTransferQueue queue = new GrainTransferQueue(4);
            queue.TryEnqueue(SandCell.CreateGrain(0, 0), 1);
            queue.TryEnqueue(SandCell.CreateGrain(0, 0), 2);

            queue.Clear();

            Assert.AreEqual(0, queue.Count);
            Assert.IsFalse(queue.TryDequeue(out ushort cell, out int column));
        }
    }
}