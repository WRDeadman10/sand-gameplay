using System;
using System.Collections.Generic;

using UnityEngine;

namespace SandGameplay.Board {
    /// <summary>The board's cell layout in world space (1 cell = 1 world unit) plus blocked wall cells.</summary>
    public sealed class BoardGrid {
        private const float OverlapEpsilon = 0.001f;

        private readonly bool[] _blocked;

        public BoardGrid(int columns, int rows, Vector2 origin, IReadOnlyList<Vector2Int> blockedCells) {
            if (columns <= 0) {
                throw new ArgumentOutOfRangeException(nameof(columns), columns, "Board needs at least one column.");
            }

            if (rows <= 0) {
                throw new ArgumentOutOfRangeException(nameof(rows), rows, "Board needs at least one row.");
            }

            Columns = columns;
            Rows = rows;
            Origin = origin;
            _blocked = new bool[columns * rows];
            if (blockedCells == null) {
                return;
            }

            for (int i = 0; i < blockedCells.Count; i++) {
                Vector2Int cell = blockedCells[i];
                if (IsInside(cell)) {
                    _blocked[(cell.y * columns) + cell.x] = true;
                }
            }
        }

        public int Columns { get; }

        public int Rows { get; }

        public Vector2 Origin { get; }

        public Rect WorldBounds {
            get {
                return new Rect(Origin.x, Origin.y, Columns, Rows);
            }
        }

        public bool IsInside(Vector2Int cell) {
            return cell.x >= 0 && cell.x < Columns && cell.y >= 0 && cell.y < Rows;
        }

        public bool IsBlocked(Vector2Int cell) {
            return !IsInside(cell) || _blocked[(cell.y * Columns) + cell.x];
        }

        public Vector2 CellToWorld(Vector2Int cell) {
            return new Vector2(Origin.x + cell.x, Origin.y + cell.y);
        }

        public Vector2Int WorldToCell(Vector2 world) {
            return new Vector2Int(Mathf.FloorToInt(world.x - Origin.x), Mathf.FloorToInt(world.y - Origin.y));
        }

        /// <summary>Clamps a footprint's bottom-left position so the footprint stays inside the board.</summary>
        public Vector2 ClampFootprint(Vector2 bottomLeft, Vector2Int footprint) {
            float maxX = Origin.x + Mathf.Max(0, Columns - footprint.x);
            float maxY = Origin.y + Mathf.Max(0, Rows - footprint.y);
            return new Vector2(Mathf.Clamp(bottomLeft.x, Origin.x, maxX), Mathf.Clamp(bottomLeft.y, Origin.y, maxY));
        }

        /// <summary>True when every cell overlapped by the rectangle is inside the board and not a wall.</summary>
        public bool IsAreaFree(Rect worldRect) {
            int minX = Mathf.FloorToInt(worldRect.xMin - Origin.x + OverlapEpsilon);
            int maxX = Mathf.CeilToInt(worldRect.xMax - Origin.x - OverlapEpsilon) - 1;
            int minY = Mathf.FloorToInt(worldRect.yMin - Origin.y + OverlapEpsilon);
            int maxY = Mathf.CeilToInt(worldRect.yMax - Origin.y - OverlapEpsilon) - 1;
            for (int y = minY; y <= maxY; y++) {
                for (int x = minX; x <= maxX; x++) {
                    if (IsBlocked(new Vector2Int(x, y))) {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>Finds the free cell closest to a footprint position among the four surrounding grid-aligned candidates.</summary>
        public bool TryFindNearestFreeCell(Vector2 bottomLeft, Vector2Int footprint, out Vector2Int cell) {
            float localX = bottomLeft.x - Origin.x;
            float localY = bottomLeft.y - Origin.y;
            int floorX = Mathf.FloorToInt(localX);
            int floorY = Mathf.FloorToInt(localY);
            cell = new Vector2Int(floorX, floorY);
            float bestDistance = float.MaxValue;
            bool found = false;
            for (int offsetY = 0; offsetY <= 1; offsetY++) {
                for (int offsetX = 0; offsetX <= 1; offsetX++) {
                    Vector2Int candidate = new Vector2Int(floorX + offsetX, floorY + offsetY);
                    Rect area = new Rect(CellToWorld(candidate), footprint);
                    if (!IsAreaFree(area)) {
                        continue;
                    }

                    float deltaX = candidate.x - localX;
                    float deltaY = candidate.y - localY;
                    float distance = (deltaX * deltaX) + (deltaY * deltaY);
                    if (distance < bestDistance) {
                        bestDistance = distance;
                        cell = candidate;
                        found = true;
                    }
                }
            }

            return found;
        }
    }
}