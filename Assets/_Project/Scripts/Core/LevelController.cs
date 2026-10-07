using System;
using System.Collections.Generic;

using SandGameplay.Board;
using SandGameplay.Container;
using SandGameplay.Data;
using SandGameplay.Interaction;
using SandGameplay.Presentation;
using SandGameplay.Sand;
using SandGameplay.Simulation;
using SandGameplay.Source;
using SandGameplay.Transfer;
using SandGameplay.UI;

using UnityEngine;

namespace SandGameplay.Core {
    /// <summary>
    /// Composition root and level state machine: builds every subsystem from the active <see cref="LevelConfig"/>,
    /// supports multi-level progression (all 11 levels), Main Menu, Level Select, and seamless restarting.
    /// </summary>
    public sealed class LevelController : MonoBehaviour {
        [Header("Levels")]
        [SerializeField] private LevelConfig defaultLevel;
        [SerializeField] private LevelConfig[] allLevels = Array.Empty<LevelConfig>();

        [Header("Scene Subsystems")]
        [SerializeField] private SandSimulationRunner simulationRunner;
        [SerializeField] private BoardView boardView;
        [SerializeField] private SandSource tank;
        [SerializeField] private ContainerPiece container;
        [SerializeField] private ContainerDragController dragController;
        [SerializeField] private CompletionSequencer completionSequencer;
        [SerializeField] private PourSprayVfx pourSpray;
        [SerializeField] private CameraFitter cameraFitter;
        [SerializeField] private HudView hud;
        [SerializeField] private LevelCompleteView levelCompleteView;
        [SerializeField] private MainMenuView mainMenuView;

        [Header("Settings")]
        [SerializeField, Range(30, 120)] private int targetFrameRate = 60;

        private LevelConfig _currentLevel;
        private int _currentLevelIndex;
        private SandFlowSimulation _flow;
        private uint _seed;
        private float _levelStartTime;
        private bool _isInitialized;

        public LevelState State { get; private set; }

        public LevelConfig CurrentLevel => _currentLevel;
        public IReadOnlyList<LevelConfig> AllLevels => allLevels;

        public void LoadLevel(int index) {
            if (allLevels == null || allLevels.Length == 0) {
                Restart();
                return;
            }

            _currentLevelIndex = Mathf.Clamp(index, 0, allLevels.Length - 1);
            _currentLevel = allLevels[_currentLevelIndex];
            CleanupCurrentLevel();
            StartLevel();
        }

        public void NextLevel() {
            if (allLevels == null || allLevels.Length == 0) {
                Restart();
                return;
            }

            int nextIndex = (_currentLevelIndex + 1) % allLevels.Length;
            Debug.Log($"[LevelController] NextLevel requested: advancing from index {_currentLevelIndex} to {nextIndex} ({allLevels[nextIndex].DisplayName}).");
            LoadLevel(nextIndex);
        }

        public void Restart() {
            if (!_isInitialized) {
                return;
            }

            CleanupCurrentLevel();
            StartLevel();
            Debug.Log($"[Level] Restarted '{_currentLevel.DisplayName}'.");
        }

        public void OpenMenu() {
            if (mainMenuView != null) {
                mainMenuView.PopulateLevelList(allLevels.Length > 0 ? allLevels : new[] { _currentLevel }, _currentLevelIndex);
                mainMenuView.Show();
                dragController.SetInputEnabled(false);
                simulationRunner.SetTransferEnabled(false);
            }
        }

        private void Awake() {
            if (!HasValidSetup()) {
                enabled = false;
                return;
            }

            Application.targetFrameRate = targetFrameRate;
            Application.runInBackground = true;

            // Build full levels list if not set in inspector
            if (allLevels == null || allLevels.Length == 0) {
                allLevels = Resources.LoadAll<LevelConfig>("Levels");
                if (allLevels == null || allLevels.Length == 0) {
                    allLevels = new[] { defaultLevel };
                }
            }

            _currentLevelIndex = 0;
            _currentLevel = (allLevels.Length > 0) ? allLevels[0] : defaultLevel;
        }

        private void OnEnable() {
            hud.RestartRequested += Restart;
            hud.MenuRequested += OpenMenu;
            levelCompleteView.PlayAgainRequested += Restart;
            levelCompleteView.NextLevelRequested += NextLevel;
            if (mainMenuView != null) {
                mainMenuView.PlayClicked += Restart;
                mainMenuView.LevelSelected += LoadLevel;
            }
        }

        private void OnDisable() {
            hud.RestartRequested -= Restart;
            hud.MenuRequested -= OpenMenu;
            levelCompleteView.PlayAgainRequested -= Restart;
            levelCompleteView.NextLevelRequested -= NextLevel;
            if (mainMenuView != null) {
                mainMenuView.PlayClicked -= Restart;
                mainMenuView.LevelSelected -= LoadLevel;
            }
        }

        private void Start() {
            StartLevel();
            if (mainMenuView != null && mainMenuView.IsOpen) {
                OpenMenu();
            }
        }

        private void OnDestroy() {
            CleanupCurrentLevel();
        }

        private readonly List<ContainerPiece> _activeContainers = new List<ContainerPiece>();
        private readonly List<ContainerPiece> _spawnedClones = new List<ContainerPiece>();

        private void CleanupCurrentLevel() {
            if (!_isInitialized) {
                return;
            }

            if (completionSequencer != null) {
                completionSequencer.Stop();
            }
            if (levelCompleteView != null) {
                levelCompleteView.Hide();
            }
            if (mainMenuView != null) {
                mainMenuView.Hide();
            }
            if (Audio.AudioManager.Instance != null) {
                Audio.AudioManager.Instance.SetPouring(false);
            }
            for (int i = _spawnedClones.Count - 1; i >= 0; i--) {
                if (_spawnedClones[i] != null && _spawnedClones[i] != container) {
                    Destroy(_spawnedClones[i].gameObject);
                }
            }
            _spawnedClones.Clear();
            _activeContainers.Clear();
            _isInitialized = false;
        }

        private void StartLevel() {
            if (_currentLevel == null) {
                _currentLevel = defaultLevel;
            }

            if (!_currentLevel.TryValidate(out string error)) {
                Debug.LogError($"[Level] {_currentLevel.name} error: {error}", this);
                enabled = false;
                return;
            }

            SimulationSettings simulation = _currentLevel.Simulation;
            SandPalette palette = _currentLevel.Palette;
            _seed = simulation.Seed != 0 ? (uint)simulation.Seed : ((uint)Environment.TickCount | 1u);
            Color32[] toneLut = palette.BuildToneLut();

            Vector2 boardOrigin = Vector2.zero;
            BoardGrid board = new BoardGrid(_currentLevel.BoardSize.x, _currentLevel.BoardSize.y, boardOrigin, _currentLevel.WallCells);
            Rect tankRect = _currentLevel.GetTankWorldRect(boardOrigin);
            boardView.Build(board, tankRect, _currentLevel.WallCells);
            if (!tank.Initialize(tankRect, simulation.GrainsPerCell, _currentLevel.TankPattern, palette, toneLut, _seed)) {
                enabled = false;
                return;
            }

            // Clean up previous clones
            for (int i = _spawnedClones.Count - 1; i >= 0; i--) {
                if (_spawnedClones[i] != null && _spawnedClones[i] != container) {
                    Destroy(_spawnedClones[i].gameObject);
                }
            }
            _spawnedClones.Clear();
            _activeContainers.Clear();

            var defs = _currentLevel.Containers;
            var colorTotals = new Dictionary<int, int>();
            var boxesPerColor = new Dictionary<int, int>();
            for (int i = 0; i < defs.Count; i++) {
                int cid = defs[i].colorId;
                if (!colorTotals.ContainsKey(cid)) {
                    colorTotals[cid] = _currentLevel.ColorRule.CountAcceptable(tank.Grid, cid);
                }
                boxesPerColor[cid] = boxesPerColor.GetValueOrDefault(cid, 0) + 1;
            }

            ContainerPiece template = container;

            for (int i = 0; i < defs.Count; i++) {
                var def = defs[i];
                ContainerPiece piece;
                if (i == 0 && container != null) {
                    piece = container;
                    piece.gameObject.SetActive(true);
                } else {
                    piece = Instantiate(template, template.transform.parent);
                    piece.name = $"Container_{def.colorId}_{i}";
                    piece.gameObject.SetActive(true);
                    _spawnedClones.Add(piece);
                }

                int totalForColor = colorTotals.GetValueOrDefault(def.colorId, 0);
                int numBoxes = boxesPerColor.GetValueOrDefault(def.colorId, 1);
                int capacity = Mathf.Max(1, totalForColor / numBoxes);

                piece.Initialize(board, tank, def.colorId, def.footprint, def.startCell, _currentLevel.CompletionThreshold, _currentLevel.AssistThreshold, simulation.GrainsPerCell, simulation.InteriorSpreadRadius, palette, toneLut, capacity, _seed ^ (uint)(i * 7919));

                ContainerPiece captured = piece;
                captured.Logic.Completed += () => HandleContainerCompleted(captured);
                captured.PathOpened += HandlePathOpened;

                _activeContainers.Add(piece);
            }

            _flow = new SandFlowSimulation(tank.Grid, _currentLevel.ColorRule, simulation.SlideChance, simulation.MaxGrainsPerTick, simulation.AssistGrainsPerTick);
            simulationRunner.Bind(_flow, _activeContainers, new FixedTickClock(simulation.TicksPerSecond, simulation.MaxTicksPerFrame));
            pourSpray.Bind(_activeContainers.Count > 0 ? _activeContainers[0] : container, simulationRunner, palette.GetSandColor(_currentLevel.ContainerColorId));

            Rect boardRect = board.WorldBounds;
            Rect content = Rect.MinMaxRect(Mathf.Min(boardRect.xMin, tankRect.xMin), boardRect.yMin, Mathf.Max(boardRect.xMax, tankRect.xMax), tankRect.yMax);
            cameraFitter.Fit(content);
            hud.SetLevelName(_currentLevel.DisplayName);
            _isInitialized = true;
            EnterPlaying();
            Debug.Log($"[Level] Started '{_currentLevel.DisplayName}': tank {tank.Grid.Width}x{tank.Grid.Height} grains, {_activeContainers.Count} containers, seed {_seed}.");
        }

        private void EnterPlaying() {
            State = LevelState.Playing;
            _levelStartTime = Time.time;
            simulationRunner.SetTransferEnabled(true);
            dragController.SetInputEnabled(true);
            tank.SetHintVisible(true);
            hud.ShowHint();
        }

        private void HandlePathOpened() {
            if (State != LevelState.Playing) {
                return;
            }

            tank.SetHintVisible(false);
            hud.HideHint();
        }

        private void HandleContainerCompleted(ContainerPiece completedPiece) {
            if (State != LevelState.Playing) {
                return;
            }

            completedPiece.FillToTop(_currentLevel.Palette);
            completionSequencer.Play(completedPiece, tank, () => HandlePieceFinished(completedPiece));
        }

        private void HandlePieceFinished(ContainerPiece completedPiece) {
            _activeContainers.Remove(completedPiece);
            completedPiece.gameObject.SetActive(false);

            if (_activeContainers.Count == 0) {
                State = LevelState.Completed;
                LevelProgress.SetLevelCleared(_currentLevelIndex);
                levelCompleteView.Show();
            }
        }

        private bool HasValidSetup() {
            if (defaultLevel == null && (allLevels == null || allLevels.Length == 0)) {
                Debug.LogError("[Level] No defaultLevel or allLevels assigned.", this);
                return false;
            }

            LevelConfig levelToCheck = defaultLevel != null ? defaultLevel : allLevels[0];
            if (!levelToCheck.TryValidate(out string error)) {
                Debug.LogError($"[Level] {error}", this);
                return false;
            }

            if (simulationRunner == null || boardView == null || tank == null || container == null || dragController == null || completionSequencer == null || pourSpray == null || cameraFitter == null || hud == null || levelCompleteView == null) {
                Debug.LogError("[Level] One or more scene references are missing on the LevelController.", this);
                return false;
            }

            return true;
        }
    }
}