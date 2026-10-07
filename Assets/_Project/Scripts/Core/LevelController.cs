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
        [SerializeField, Min(64)] private int transferQueueCapacity = 4096;

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
            LoadLevel(nextIndex);
        }

        public void Restart() {
            if (!_isInitialized) {
                return;
            }

            completionSequencer.Stop();
            levelCompleteView.Hide();
            if (mainMenuView != null) {
                mainMenuView.Hide();
            }
            simulationRunner.ResetClock();
            _flow.Reset();
            tank.ResetFill();
            container.ResetToStart(CountCapacity());
            EnterPlaying();
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
        }

        private void OnDestroy() {
            CleanupCurrentLevel();
        }

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
            if (container != null && container.Logic != null) {
                container.Logic.Completed -= HandleContainerCompleted;
                container.PathOpened -= HandlePathOpened;
            }
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

            int capacity = CountCapacity();
            if (capacity <= 0) {
                Debug.LogError($"[Level] '{_currentLevel.name}' has no grains the box can accept; check the tank pattern and box color.", this);
                enabled = false;
                return;
            }

            if (!container.Initialize(board, tank, _currentLevel, palette, toneLut, capacity, _seed)) {
                enabled = false;
                return;
            }

            GrainTransferQueue queue = new GrainTransferQueue(transferQueueCapacity);
            _flow = new SandFlowSimulation(tank.Grid, container.Interior, container.Logic, container.Spawner, queue, _currentLevel.ColorRule, _currentLevel.ContainerColorId, simulation.SlideChance, simulation.MaxGrainsPerTick, simulation.AssistGrainsPerTick);
            simulationRunner.Bind(_flow, container, new FixedTickClock(simulation.TicksPerSecond, simulation.MaxTicksPerFrame));
            pourSpray.Bind(container, simulationRunner, palette.GetSandColor(_currentLevel.ContainerColorId));
            container.Logic.Completed += HandleContainerCompleted;
            container.PathOpened += HandlePathOpened;

            Rect boardRect = board.WorldBounds;
            Rect content = Rect.MinMaxRect(Mathf.Min(boardRect.xMin, tankRect.xMin), boardRect.yMin, Mathf.Max(boardRect.xMax, tankRect.xMax), tankRect.yMax);
            cameraFitter.Fit(content);
            hud.SetLevelName(_currentLevel.DisplayName);
            _isInitialized = true;
            EnterPlaying();
            Debug.Log($"[Level] Started '{_currentLevel.DisplayName}': tank {tank.Grid.Width}x{tank.Grid.Height} grains, box capacity {capacity} grains, seed {_seed}.");
        }

        private int CountCapacity() {
            return _currentLevel.ColorRule.CountAcceptable(tank.Grid, _currentLevel.ContainerColorId);
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

        private void HandleContainerCompleted() {
            if (State != LevelState.Playing) {
                return;
            }

            State = LevelState.Completing;
            dragController.SetInputEnabled(false);
            simulationRunner.SetTransferEnabled(false);
            tank.SetHintVisible(false);
            hud.HideHint();
            container.FillToTop(_currentLevel.Palette);
            Debug.Log($"[Level] Box filled in {Time.time - _levelStartTime:0.0} s.");
            completionSequencer.Play(container, tank, HandleSequenceFinished);
        }

        private void HandleSequenceFinished() {
            State = LevelState.Completed;
            levelCompleteView.Show();
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