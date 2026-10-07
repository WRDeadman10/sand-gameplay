using System;

using SandGameplay.Board;
using SandGameplay.Data;
using SandGameplay.Sand;
using SandGameplay.Source;
using SandGameplay.Transfer;
using SandGameplay.Utilities;

using UnityEngine;

namespace SandGameplay.Container {
    /// <summary>
    /// The draggable open-top box. Owns the interior sand grid and fill logic, moves under board constraints and
    /// raises <see cref="PathOpened"/> / <see cref="PathClosed"/> when it docks under, or leaves, the tank.
    /// The transform sits at the footprint's bottom-left corner.
    /// </summary>
    [DefaultExecutionOrder(-90)]
    public sealed class ContainerPiece : MonoBehaviour {
        [SerializeField] private ContainerVisuals visuals;
        [SerializeField] private SandGridView interiorView;
        [SerializeField] private ContainerLabelView labelView;
        [SerializeField, Min(1), Tooltip("Side wall thickness in grains.")] private int wallGrains = 4;
        [SerializeField, Min(1), Tooltip("Front face (bottom) thickness in grains.")] private int bottomGrains = 5;
        [SerializeField, Min(1), Tooltip("Top rim thickness in grains; poured grains fall through it.")] private int rimGrains = 3;
        [SerializeField, Range(0, SandCell.MaxRegularTone), Tooltip("Tone used to top up the interior on completion.")] private int topUpTone = 7;
        [SerializeField, Min(1f)] private float followSharpness = 28f;
        [SerializeField, Min(0.01f)] private float snapDuration = 0.12f;
        [SerializeField, Range(0.01f, 0.5f)] private float dockTolerance = 0.05f;
        [SerializeField, Range(0.05f, 0.5f)] private float maxStepSize = 0.2f;

        private BoardGrid _board;
        private SandSource _tank;
        private Vector2Int _footprint;
        private Vector2Int _startCell;
        private int _grainsPerCell;
        private Vector2 _position;
        private Vector2 _dragTarget;
        private bool _isDragging;
        private bool _isSnapping;
        private Vector2 _snapFrom;
        private Vector2 _snapTo;
        private float _snapElapsed;
        private OutletSpan _outletSpan;

        public event Action PathOpened;

        public event Action PathClosed;

        public SandContainer Logic { get; private set; }

        public SandGrid Interior { get; private set; }

        public InteriorSpawner Spawner { get; private set; }

        public int ColorId { get; private set; }

        public bool IsDocked { get; private set; }

        public bool IsDragging {
            get {
                return _isDragging;
            }
        }

        public ContainerVisuals Visuals {
            get {
                return visuals;
            }
        }

        /// <summary>Bottom-left corner of the footprint in world space.</summary>
        public Vector2 Position {
            get {
                return _position;
            }
        }

        public OutletSpan OutletSpan {
            get {
                return _outletSpan;
            }
        }

        public Rect FootprintWorldRect {
            get {
                return new Rect(_position, _footprint);
            }
        }

        /// <summary>The box opening (between the side walls, at the rim) in world space.</summary>
        public Rect OpeningWorldRect {
            get {
                float grainSize = 1f / _grainsPerCell;
                float wall = wallGrains * grainSize;
                float rim = rimGrains * grainSize;
                return new Rect(_position.x + wall, _position.y + _footprint.y - rim, _footprint.x - (2f * wall), rim);
            }
        }

        public bool Initialize(BoardGrid board, SandSource tank, LevelConfig level, SandPalette palette, Color32[] toneLut, int capacity, uint seed) {
            if (visuals == null || interiorView == null || labelView == null || !visuals.HasReferences()) {
                Debug.LogError("[ContainerPiece] Missing visual references; check the Container prefab.", this);
                return false;
            }

            if (board == null || tank == null || level == null || palette == null || toneLut == null) {
                Debug.LogError("[ContainerPiece] Initialize needs a board, tank, level, palette and tone table.", this);
                return false;
            }

            _board = board;
            _tank = tank;
            _footprint = level.ContainerFootprint;
            _startCell = level.ContainerStartCell;
            _grainsPerCell = level.Simulation.GrainsPerCell;
            ColorId = level.ContainerColorId;

            int interiorWidth = (_footprint.x * _grainsPerCell) - (2 * wallGrains);
            int fillRows = (_footprint.y * _grainsPerCell) - bottomGrains - rimGrains;
            if (interiorWidth <= 0 || fillRows <= 0) {
                Debug.LogError($"[ContainerPiece] Walls ({wallGrains}/{bottomGrains}/{rimGrains} grains) do not fit a {_footprint} footprint at {_grainsPerCell} grains per cell.", this);
                return false;
            }

            Interior = new SandGrid(interiorWidth, fillRows + rimGrains, seed ^ 0x6A09E667u);
            Spawner = new InteriorSpawner(Interior, fillRows, level.Simulation.InteriorSpreadRadius, seed ^ 0xBB67AE85u);
            Logic = new SandContainer(level.CompletionThreshold, level.AssistThreshold);

            float grainSize = 1f / _grainsPerCell;
            Vector2 footprintSize = new Vector2(_footprint.x, _footprint.y);
            Rect interiorLocalRect = new Rect(
                (-footprintSize.x * 0.5f) + (wallGrains * grainSize),
                (-footprintSize.y * 0.5f) + (bottomGrains * grainSize),
                interiorWidth * grainSize,
                fillRows * grainSize);
            visuals.Configure(footprintSize, interiorLocalRect, palette.GetBodyColor(ColorId));
            interiorView.Bind(Interior, toneLut, _grainsPerCell);
            labelView.Bind(Logic);
            ResetToStart(capacity);
            return true;
        }

        public void ResetToStart(int capacity) {
            Interior.Clear();
            Spawner.Reset(capacity);
            Logic.Reset(capacity);
            _isDragging = false;
            _isSnapping = false;
            _position = _board.CellToWorld(_startCell);
            _dragTarget = _position;
            ApplyPosition();
            visuals.ResetVisuals();
            UpdateDockState();
        }

        /// <summary>Tops the interior up so the completed box reads as exactly full.</summary>
        public void FillToTop(SandPalette palette) {
            Spawner.FillToTop(palette, ColorId, topUpTone);
        }

        public bool ContainsPoint(Vector2 worldPoint, float padding) {
            Rect area = FootprintWorldRect;
            return worldPoint.x >= area.xMin - padding && worldPoint.x <= area.xMax + padding && worldPoint.y >= area.yMin - padding && worldPoint.y <= area.yMax + padding;
        }

        public void BeginDrag() {
            _isDragging = true;
            _isSnapping = false;
            _dragTarget = _position;
            visuals.SetSelected(true);
        }

        /// <summary>Sets the desired bottom-left position; the box follows it under board constraints.</summary>
        public void DragTo(Vector2 bottomLeft) {
            _dragTarget = bottomLeft;
        }

        public void EndDrag() {
            if (!_isDragging) {
                return;
            }

            _isDragging = false;
            visuals.SetSelected(false);
            BeginSnap();
        }

        private void Update() {
            if (_board == null) {
                return;
            }

            if (_isDragging) {
                Vector2 target = _board.ClampFootprint(_dragTarget, _footprint);
                float blend = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
                _position = MoveConstrained(_position, Vector2.Lerp(_position, target, blend));
            } else if (_isSnapping) {
                _snapElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(_snapElapsed / snapDuration);
                _position = Vector2.LerpUnclamped(_snapFrom, _snapTo, Easing.OutQuad(t));
                if (t >= 1f) {
                    _position = _snapTo;
                    _isSnapping = false;
                }
            }

            ApplyPosition();
            UpdateDockState();
        }

        private void BeginSnap() {
            if (!_board.TryFindNearestFreeCell(_position, _footprint, out Vector2Int cell)) {
                return;
            }

            _snapFrom = _position;
            _snapTo = _board.CellToWorld(cell);
            _snapElapsed = 0f;
            _isSnapping = (_snapTo - _snapFrom).sqrMagnitude > 0.000001f;
        }

        /// <summary>Moves along X then Y in small steps so the box slides along walls instead of passing through them.</summary>
        private Vector2 MoveConstrained(Vector2 from, Vector2 to) {
            Vector2 result = from;
            result.x += MoveAlongAxis(result, to.x - from.x, true);
            result.y += MoveAlongAxis(result, to.y - from.y, false);
            return result;
        }

        private float MoveAlongAxis(Vector2 start, float delta, bool horizontal) {
            float distance = Mathf.Abs(delta);
            if (distance < 0.00001f) {
                return 0f;
            }

            float direction = Mathf.Sign(delta);
            float moved = 0f;
            while (moved < distance) {
                float next = Mathf.Min(distance, moved + maxStepSize);
                Vector2 candidate = horizontal
                    ? new Vector2(start.x + (direction * next), start.y)
                    : new Vector2(start.x, start.y + (direction * next));
                if (!_board.IsAreaFree(new Rect(candidate, _footprint))) {
                    break;
                }

                moved = next;
            }

            return direction * moved;
        }

        private void ApplyPosition() {
            transform.position = new Vector3(_position.x, _position.y, 0f);
        }

        private void UpdateDockState() {
            float openingMinX = _position.x + (wallGrains / (float)_grainsPerCell);
            float boxTopY = _position.y + _footprint.y;
            bool docked = DockResolver.TryResolve(openingMinX, boxTopY, Interior.Width, _tank.InteriorWorldRect, _grainsPerCell, dockTolerance, _tank.Grid.Width, out OutletSpan span);
            _outletSpan = docked ? span : default(OutletSpan);
            if (docked == IsDocked) {
                return;
            }

            IsDocked = docked;
            if (docked) {
                visuals.PlayDockBump();
                PathOpened?.Invoke();
            } else {
                PathClosed?.Invoke();
            }
        }
    }
}