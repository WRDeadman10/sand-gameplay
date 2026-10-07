using NUnit.Framework;

using SandGameplay.Board;

using UnityEngine;

namespace SandGameplay.Tests {
    public sealed class BoardGridTests {
        private static readonly Vector2 Origin = new Vector2(10f, -2f);
        private static readonly Vector2Int Footprint = new Vector2Int(2, 2);

        [Test]
        public void CellToWorld_AndWorldToCell_RoundTrip() {
            BoardGrid board = new BoardGrid(6, 7, Origin, null);
            Vector2Int cell = new Vector2Int(3, 5);

            Vector2 world = board.CellToWorld(cell);

            Assert.AreEqual(new Vector2(13f, 3f), world);
            Assert.AreEqual(cell, board.WorldToCell(world + new Vector2(0.5f, 0.5f)));
        }

        [Test]
        public void IsAreaFree_InsideEmptyBoard_IsTrue() {
            BoardGrid board = new BoardGrid(6, 7, Vector2.zero, null);

            Assert.IsTrue(board.IsAreaFree(new Rect(0f, 0f, 2f, 2f)));
            Assert.IsTrue(board.IsAreaFree(new Rect(3.5f, 4.2f, 2f, 2f)));
        }

        [Test]
        public void IsAreaFree_OutsideBoardOrOverWall_IsFalse() {
            BoardGrid board = new BoardGrid(6, 7, Vector2.zero, new Vector2Int[] { new Vector2Int(3, 3) });

            Assert.IsFalse(board.IsAreaFree(new Rect(-0.5f, 0f, 2f, 2f)));
            Assert.IsFalse(board.IsAreaFree(new Rect(5f, 0f, 2f, 2f)));
            Assert.IsFalse(board.IsAreaFree(new Rect(2.5f, 2.5f, 2f, 2f)));
        }

        [Test]
        public void IsAreaFree_TouchingWallEdge_IsTrue() {
            BoardGrid board = new BoardGrid(6, 7, Vector2.zero, new Vector2Int[] { new Vector2Int(3, 3) });

            Assert.IsTrue(board.IsAreaFree(new Rect(1f, 3f, 2f, 2f)));
        }

        [Test]
        public void ClampFootprint_KeepsFootprintInsideBoard() {
            BoardGrid board = new BoardGrid(6, 7, Origin, null);

            Vector2 clamped = board.ClampFootprint(new Vector2(100f, -100f), Footprint);

            Assert.AreEqual(new Vector2(14f, -2f), clamped);
        }

        [Test]
        public void TryFindNearestFreeCell_PicksClosestFreeCandidate() {
            BoardGrid board = new BoardGrid(6, 7, Vector2.zero, new Vector2Int[] { new Vector2Int(2, 5) });

            bool found = board.TryFindNearestFreeCell(new Vector2(2.3f, 4.8f), Footprint, out Vector2Int cell);

            Assert.IsTrue(found);
            Assert.AreEqual(new Vector2Int(3, 5), cell);
        }
    }
}