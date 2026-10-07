using System;

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
    /// Composition root and level state machine: builds every subsystem from the <see cref="LevelConfig"/>,
    /// reacts to the box completing, and restarts in place without reloading the scene.
    /// </summary>
    public sealed class LevelController : MonoBehaviour {
        [SerializeField] private LevelConfig level;
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
        [SerializeField, Range(30, 120)] private int targetFrameRate = 60;
        [SerializeField, Min(64)] private int transferQueueCapacity = 4096;

        private SandFlowSimulation _flow;
        private uint _seed;
        private float _levelStartTime;
        private bool _isInitialized;

        public LevelState State { get; private set; }

        public void Restart() {
            if (!_isInitialized) {
                return;
            }

            completionSequencer.Stop();
            levelCompleteView.Hide();
            simulationRunner.ResetClock();
            _flow.Reset();
            tank.ResetFill();
            container.ResetToStart(CountCapacity());
            EnterPlaying();
            Debug.Log("[Level] Restarted.");
        }

        private void Awake() {
            if (!HasValidSetup()) {
                enabled = false;
                return;
            }

            Application.targetFrameRate = targetFrameRate;
        }

        private void OnEnable() {
            hud.RestartRequested += Restart;
            levelCompleteView.PlayAgainRequested += Restart;
        }

        private void OnDisable() {
            hud.RestartRequested -= Restart;
            levelCompleteView.PlayAgainRequested -= Restart;
        }

        private void Start() {
            StartLevel();
        }

        private void OnDestroy() {
            if (!_isInitialized) {
                return;
            }

            container.Logic.Completed -= HandleContainerCompleted;
            container.PathOpened -= HandlePathOpened;
        }

        private void StartLevel() {
            SimulationSettings simulation = level.Simulation;
            SandPalette palette = level.Palette;
            _seed = simulation.Seed != 0 ? (uint)simulation.Seed : ((uint)Environment.TickCount | 1u);
            Color32[] toneLut = palette.BuildToneLut();

            Vector2 boardOrigin = Vector2.zero;
            BoardGrid board = new BoardGrid(level.BoardSize.x, level.BoardSize.y, boardOrigin, level.WallCells);
            Rect tankRect = level.GetTankWorldRect(boardOrigin);
            boardView.Build(board, tankRect, level.WallCells);
            if (!tank.Initialize(tankRect, simulation.GrainsPerCell, level.TankPattern, palette, toneLut, _seed)) {
                enabled = false;
                return;
            }

            int capacity = CountCapacity();
            if (capacity <= 0) {
                Debug.LogError($"[Level] '{level.name}' has no grains the box can accept; check the tank pattern and box color.", this);
                enabled = false;
                return;
            }

            if (!container.Initialize(board, tank, level, palette, toneLut, capacity, _seed)) {
                enabled = false;
                return;
            }

            GrainTransferQueue queue = new GrainTransferQueue(transferQueueCapacity);
            _flow = new SandFlowSimulation(tank.Grid, container.Interior, container.Logic, container.Spawner, queue, level.ColorRule, level.ContainerColorId, simulation.SlideChance, simulation.MaxGrainsPerTick, simulation.AssistGrainsPerTick);
            simulationRunner.Bind(_flow, container, new FixedTickClock(simulation.TicksPerSecond, simulation.MaxTicksPerFrame));
            pourSpray.Bind(container, simulationRunner, palette.GetSandColor(level.ContainerColorId));
            container.Logic.Completed += HandleContainerCompleted;
            container.PathOpened += HandlePathOpened;

            Rect boardRect = board.WorldBounds;
            Rect content = Rect.MinMaxRect(Mathf.Min(boardRect.xMin, tankRect.xMin), boardRect.yMin, Mathf.Max(boardRect.xMax, tankRect.xMax), tankRect.yMax);
            cameraFitter.Fit(content);
            hud.SetLevelName(level.DisplayName);
            _isInitialized = true;
            EnterPlaying();
            Debug.Log($"[Level] Started '{level.DisplayName}': tank {tank.Grid.Width}x{tank.Grid.Height} grains, box capacity {capacity} grains, seed {_seed}.");
        }

        private int CountCapacity() {
            return level.ColorRule.CountAcceptable(tank.Grid, level.ContainerColorId);
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
            container.FillToTop(level.Palette);
            Debug.Log($"[Level] Box filled in {Time.time - _levelStartTime:0.0} s.");
            completionSequencer.Play(container, tank, HandleSequenceFinished);
        }

        private void HandleSequenceFinished() {
            State = LevelState.Completed;
            levelCompleteView.Show();
        }

        private bool HasValidSetup() {
            if (level == null) {
                Debug.LogError("[Level] No LevelConfig assigned.", this);
                return false;
            }

            if (!level.TryValidate(out string error)) {
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