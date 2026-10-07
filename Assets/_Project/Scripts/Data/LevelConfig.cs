using System;
using System.Collections.Generic;

using SandGameplay.Sand;
using SandGameplay.Source;
using SandGameplay.Transfer;

using UnityEngine;

namespace SandGameplay.Data {
    /// <summary>Defines one draggable container piece in a level.</summary>
    [Serializable]
    public struct ContainerDefinition {
        [Min(0)] public int colorId;
        public Vector2Int footprint;
        public Vector2Int startCell;

        public ContainerDefinition(int colorId, Vector2Int footprint, Vector2Int startCell) {
            this.colorId = colorId;
            this.footprint = footprint;
            this.startCell = startCell;
        }
    }

    /// <summary>Everything that defines a level: board, tank, box, rules and tuning references.</summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Sand Gameplay/Level Config")]
    public sealed class LevelConfig : ScriptableObject {
        [SerializeField] private string displayName = "Level 1";
        [SerializeField] private Vector2Int boardSize = new Vector2Int(6, 7);
        [SerializeField] private Vector2Int[] wallCells = Array.Empty<Vector2Int>();
        [SerializeField, Tooltip("Tank interior size in cells; it sits directly on the board's top edge.")] private Vector2Int tankSize = new Vector2Int(6, 2);
        [SerializeField, Min(0)] private int tankColumnOffset;
        [SerializeField] private TankFillPattern tankPattern = new TankFillPattern();
        [SerializeField] private Vector2Int containerFootprint = new Vector2Int(2, 2);
        [SerializeField] private Vector2Int containerStartCell = new Vector2Int(2, 0);
        [SerializeField, Min(0)] private int containerColorId;
        [SerializeField] private ContainerDefinition[] containers = Array.Empty<ContainerDefinition>();
        [SerializeField] private ContainerColorRule colorRule = ContainerColorRule.MatchColor;
        [SerializeField, Range(0.5f, 1f)] private float completionThreshold = 1f;
        [SerializeField, Range(0.5f, 1f), Tooltip("While docked above this fill ratio, the remaining acceptable grains are absorbed.")] private float assistThreshold = 0.97f;
        [SerializeField] private SandPalette palette;
        [SerializeField] private SimulationSettings simulation;

        public string DisplayName {
            get {
                return displayName;
            }
        }

        public Vector2Int BoardSize {
            get {
                return boardSize;
            }
        }

        public IReadOnlyList<Vector2Int> WallCells {
            get {
                return wallCells;
            }
        }

        public Vector2Int TankSize {
            get {
                return tankSize;
            }
        }

        public TankFillPattern TankPattern {
            get {
                return tankPattern;
            }
        }

        public Vector2Int ContainerFootprint {
            get {
                return containerFootprint;
            }
        }

        public Vector2Int ContainerStartCell {
            get {
                return containerStartCell;
            }
        }

        public int ContainerColorId {
            get {
                return containerColorId;
            }
        }

        public IReadOnlyList<ContainerDefinition> Containers {
            get {
                if (containers != null && containers.Length > 0) {
                    return containers;
                }
                return new[] { new ContainerDefinition(containerColorId, containerFootprint, containerStartCell) };
            }
        }

        public ContainerColorRule ColorRule {
            get {
                return colorRule;
            }
        }

        public float CompletionThreshold {
            get {
                return completionThreshold;
            }
        }

        public float AssistThreshold {
            get {
                return assistThreshold;
            }
        }

        public SandPalette Palette {
            get {
                return palette;
            }
        }

        public SimulationSettings Simulation {
            get {
                return simulation;
            }
        }

        /// <summary>The tank interior in world space for a board whose bottom-left corner is <paramref name="boardOrigin"/>.</summary>
        public Rect GetTankWorldRect(Vector2 boardOrigin) {
            return new Rect(boardOrigin.x + tankColumnOffset, boardOrigin.y + boardSize.y, tankSize.x, tankSize.y);
        }

        public bool TryValidate(out string error) {
            if (palette == null) {
                error = $"Level '{name}' has no SandPalette assigned.";
                return false;
            }

            if (simulation == null) {
                error = $"Level '{name}' has no SimulationSettings assigned.";
                return false;
            }

            if (boardSize.x < 1 || boardSize.y < 1) {
                error = $"Level '{name}' board size {boardSize} must be at least 1x1.";
                return false;
            }

            if (tankSize.x < 1 || tankSize.y < 1 || tankColumnOffset + tankSize.x > boardSize.x) {
                error = $"Level '{name}' tank ({tankSize} at column {tankColumnOffset}) must fit above the {boardSize.x}-column board.";
                return false;
            }

            if (containerFootprint.x < 1 || containerFootprint.y < 1 || containerFootprint.x > boardSize.x || containerFootprint.y > boardSize.y) {
                error = $"Level '{name}' box footprint {containerFootprint} must fit the {boardSize} board.";
                return false;
            }

            if (containerStartCell.x < 0 || containerStartCell.y < 0 || containerStartCell.x + containerFootprint.x > boardSize.x || containerStartCell.y + containerFootprint.y > boardSize.y) {
                error = $"Level '{name}' box start cell {containerStartCell} places the box outside the board.";
                return false;
            }

            if (!palette.IsValidColorId(containerColorId)) {
                error = $"Level '{name}' box color id {containerColorId} is not in palette '{palette.name}'.";
                return false;
            }

            if (assistThreshold > completionThreshold) {
                error = $"Level '{name}' assist threshold ({assistThreshold}) must not exceed the completion threshold ({completionThreshold}).";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}