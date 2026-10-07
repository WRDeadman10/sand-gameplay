using UnityEngine;

namespace SandGameplay.Sand {
    /// <summary>Tuning values for the sand automaton and the outlet.</summary>
    [CreateAssetMenu(fileName = "SimulationSettings", menuName = "Sand Gameplay/Simulation Settings")]
    public sealed class SimulationSettings : ScriptableObject {
        [SerializeField, Range(8, 48), Tooltip("Grains per board-cell edge (grid resolution).")] private int grainsPerCell = 24;
        [SerializeField, Range(30, 240), Tooltip("Automaton ticks per second (fall speed = 1 grain per tick).")] private int ticksPerSecond = 120;
        [SerializeField, Range(1, 8), Tooltip("Cap on ticks per frame; excess time is dropped after a hitch.")] private int maxTicksPerFrame = 4;
        [SerializeField, Range(0.1f, 1f), Tooltip("Chance a blocked grain slides diagonally this tick; lower feels heavier.")] private float slideChance = 1f;
        [SerializeField, Min(0), Tooltip("Outlet budget per tick; 0 = the whole outlet span.")] private int maxGrainsPerTick;
        [SerializeField, Min(1), Tooltip("Assist absorb speed once the box passes the assist threshold.")] private int assistGrainsPerTick = 24;
        [SerializeField, Min(0), Tooltip("Random horizontal spread when grains enter the box.")] private int interiorSpreadRadius = 3;
        [SerializeField, Tooltip("0 = derive a seed from the clock at level start.")] private int seed;

        public int GrainsPerCell {
            get {
                return grainsPerCell;
            }
        }

        public int TicksPerSecond {
            get {
                return ticksPerSecond;
            }
        }

        public int MaxTicksPerFrame {
            get {
                return maxTicksPerFrame;
            }
        }

        public float SlideChance {
            get {
                return slideChance;
            }
        }

        public int MaxGrainsPerTick {
            get {
                return maxGrainsPerTick;
            }
        }

        public int AssistGrainsPerTick {
            get {
                return assistGrainsPerTick;
            }
        }

        public int InteriorSpreadRadius {
            get {
                return interiorSpreadRadius;
            }
        }

        public int Seed {
            get {
                return seed;
            }
        }
    }
}