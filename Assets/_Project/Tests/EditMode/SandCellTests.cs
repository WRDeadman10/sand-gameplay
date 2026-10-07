using NUnit.Framework;

using SandGameplay.Sand;

namespace SandGameplay.Tests {
    public sealed class SandCellTests {
        [Test]
        public void CreateGrain_RoundTripsColorAndTone() {
            ushort cell = SandCell.CreateGrain(37, 11);

            Assert.IsTrue(SandCell.IsGrain(cell));
            Assert.AreEqual(37, SandCell.GetColorId(cell));
            Assert.AreEqual(11, SandCell.GetTone(cell));
            Assert.AreEqual((37 * SandCell.ToneCount) + 11, SandCell.GetLutIndex(cell));
        }

        [Test]
        public void CreateGrain_OutOfRangeValues_AreClamped() {
            ushort cell = SandCell.CreateGrain(500, -3);

            Assert.AreEqual(SandCell.MaxColorCount - 1, SandCell.GetColorId(cell));
            Assert.AreEqual(0, SandCell.GetTone(cell));
        }

        [Test]
        public void Empty_IsNotAGrain() {
            Assert.IsFalse(SandCell.IsGrain(SandCell.Empty));
        }

        [Test]
        public void CreateGrain_FirstColorFirstTone_IsStillDistinctFromEmpty() {
            ushort cell = SandCell.CreateGrain(0, 0);

            Assert.AreNotEqual(SandCell.Empty, cell);
            Assert.IsTrue(SandCell.IsGrain(cell));
        }
    }
}