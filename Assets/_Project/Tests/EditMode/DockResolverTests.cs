using NUnit.Framework;

using SandGameplay.Transfer;

using UnityEngine;

namespace SandGameplay.Tests {
    public sealed class DockResolverTests {
        private const int GrainsPerCell = 24;
        private const int TankWidth = 144;
        private const int InteriorWidth = 40;
        private const float Tolerance = 0.05f;
        private const float WallOffset = 4f / GrainsPerCell;
        private static readonly Rect TankRect = new Rect(0f, 7f, 6f, 2f);

        [Test]
        public void TryResolve_BoxTopAtTankBottom_IsDocked_WithFullSpan() {
            bool docked = DockResolver.TryResolve(2f + WallOffset, 7f, InteriorWidth, TankRect, GrainsPerCell, Tolerance, TankWidth, out OutletSpan span);

            Assert.IsTrue(docked);
            Assert.AreEqual(52, span.TankStartX);
            Assert.AreEqual(92, span.TankEndX);
            Assert.AreEqual(52, span.InteriorOffsetX);
        }

        [Test]
        public void TryResolve_BoxTopBelowTolerance_IsNotDocked() {
            bool docked = DockResolver.TryResolve(2f + WallOffset, 6.9f, InteriorWidth, TankRect, GrainsPerCell, Tolerance, TankWidth, out OutletSpan span);

            Assert.IsFalse(docked);
            Assert.IsTrue(span.IsEmpty);
        }

        [Test]
        public void TryResolve_OpeningPastTankEdge_ClampsSpanToTank() {
            bool docked = DockResolver.TryResolve(-0.5f, 7f, InteriorWidth, TankRect, GrainsPerCell, Tolerance, TankWidth, out OutletSpan span);

            Assert.IsTrue(docked);
            Assert.AreEqual(0, span.TankStartX);
            Assert.AreEqual(28, span.TankEndX);
            Assert.AreEqual(-12, span.InteriorOffsetX);
        }

        [Test]
        public void TryResolve_OpeningOutsideTank_IsNotDocked() {
            bool docked = DockResolver.TryResolve(6.5f, 7f, InteriorWidth, TankRect, GrainsPerCell, Tolerance, TankWidth, out OutletSpan span);

            Assert.IsFalse(docked);
        }

        [Test]
        public void TryResolve_SpanFollowsOpeningPosition() {
            DockResolver.TryResolve(WallOffset, 7f, InteriorWidth, TankRect, GrainsPerCell, Tolerance, TankWidth, out OutletSpan left);
            DockResolver.TryResolve(4f + WallOffset, 7f, InteriorWidth, TankRect, GrainsPerCell, Tolerance, TankWidth, out OutletSpan right);

            Assert.AreEqual(4, left.TankStartX);
            Assert.AreEqual(100, right.TankStartX);
            Assert.AreEqual(InteriorWidth, right.Width);
        }
    }
}