using System;
using System.Collections.Generic;

using UnityEngine;

namespace SandGameplay.Board {
    /// <summary>Draws the board panel (behind board and tank), the tiled cells and any wall cells.</summary>
    public sealed class BoardView : MonoBehaviour {
        [SerializeField] private SpriteRenderer panelRenderer;
        [SerializeField] private SpriteRenderer cellsRenderer;
        [SerializeField, Tooltip("Disabled template cloned for each wall cell.")] private SpriteRenderer wallTemplate;
        [SerializeField, Min(0f)] private float panelPadding = 0.22f;

        private readonly List<SpriteRenderer> _walls = new List<SpriteRenderer>();

        public void Build(BoardGrid board, Rect tankWorldRect, IReadOnlyList<Vector2Int> wallCells) {
            if (board == null) {
                throw new ArgumentNullException(nameof(board));
            }

            if (panelRenderer == null || cellsRenderer == null) {
                Debug.LogError("[BoardView] Panel and cell renderers must be assigned.", this);
                return;
            }

            Rect boardRect = board.WorldBounds;
            Rect content = Rect.MinMaxRect(
                Mathf.Min(boardRect.xMin, tankWorldRect.xMin),
                Mathf.Min(boardRect.yMin, tankWorldRect.yMin),
                Mathf.Max(boardRect.xMax, tankWorldRect.xMax),
                Mathf.Max(boardRect.yMax, tankWorldRect.yMax));
            panelRenderer.size = content.size + (Vector2.one * (panelPadding * 2f));
            panelRenderer.transform.position = new Vector3(content.center.x, content.center.y, 0f);
            cellsRenderer.size = boardRect.size;
            cellsRenderer.transform.position = new Vector3(boardRect.center.x, boardRect.center.y, 0f);
            BuildWalls(board, wallCells);
        }

        private void BuildWalls(BoardGrid board, IReadOnlyList<Vector2Int> wallCells) {
            for (int i = 0; i < _walls.Count; i++) {
                Destroy(_walls[i].gameObject);
            }

            _walls.Clear();
            if (wallCells == null || wallCells.Count == 0) {
                return;
            }

            if (wallTemplate == null) {
                Debug.LogError("[BoardView] Walls are configured but no wall template is assigned.", this);
                return;
            }

            for (int i = 0; i < wallCells.Count; i++) {
                Vector2 cellOrigin = board.CellToWorld(wallCells[i]);
                SpriteRenderer wall = Instantiate(wallTemplate, transform);
                wall.gameObject.SetActive(true);
                wall.transform.position = new Vector3(cellOrigin.x + 0.5f, cellOrigin.y + 0.5f, 0f);
                _walls.Add(wall);
            }
        }
    }
}