using System;

using SandGameplay.Container;
using SandGameplay.Sand;
using SandGameplay.Transfer;

namespace SandGameplay.Simulation {
    /// <summary>
    /// One simulation tick of the whole sand path: step the tank, drain accepted grains through the outlet when
    /// the box is docked, account for them and spawn their visuals in the box, then step the box interior.
    /// </summary>
    public sealed class SandFlowSimulation {
        private readonly SandGrid _tank;
        private readonly SandGrid _interior;
        private readonly SandContainer _container;
        private readonly InteriorSpawner _spawner;
        private readonly GrainTransferQueue _queue;
        private readonly ContainerColorRule _rule;
        private readonly int _colorId;
        private readonly float _slideChance;
        private readonly int _maxGrainsPerTick;
        private readonly int _assistGrainsPerTick;
        private bool _scanLeftToRight = true;
        private int _unqueuedGrains;

        public SandFlowSimulation(SandGrid tank, SandGrid interior, SandContainer container, InteriorSpawner spawner, GrainTransferQueue queue, ContainerColorRule rule, int colorId, float slideChance, int maxGrainsPerTick, int assistGrainsPerTick) {
            _tank = tank ?? throw new ArgumentNullException(nameof(tank));
            _interior = interior ?? throw new ArgumentNullException(nameof(interior));
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _spawner = spawner ?? throw new ArgumentNullException(nameof(spawner));
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _rule = rule;
            _colorId = colorId;
            _slideChance = slideChance;
            _maxGrainsPerTick = maxGrainsPerTick;
            _assistGrainsPerTick = Math.Max(1, assistGrainsPerTick);
        }

        /// <summary>Grains that left the tank during the last tick (outlet plus assist).</summary>
        public int TransferredLastTick { get; private set; }

        public void Tick(bool isDocked, OutletSpan span, bool transferEnabled) {
            _tank.Step(_slideChance);
            TransferredLastTick = 0;
            if (transferEnabled && isDocked && !_container.IsComplete) {
                TransferredLastTick = SandOutlet.Transfer(_tank, span, _rule, _colorId, _maxGrainsPerTick, _scanLeftToRight, _queue);
                _scanLeftToRight = !_scanLeftToRight;
                if (_container.IsAssistActive) {
                    TransferredLastTick += AbsorbRemaining(span);
                }
            }

            DrainQueue();
            _interior.Step(_slideChance);
        }

        public void Reset() {
            _queue.Clear();
            _unqueuedGrains = 0;
            TransferredLastTick = 0;
        }

        private void DrainQueue() {
            int received = _unqueuedGrains;
            _unqueuedGrains = 0;
            while (_queue.TryDequeue(out ushort cell, out int column)) {
                _spawner.Spawn(cell, column);
                received++;
            }

            if (received > 0) {
                _container.AddReceived(received);
            }
        }

        /// <summary>
        /// Near completion, pulls the remaining acceptable grains (lowest rows first) toward the box a few per tick,
        /// so the player never has to hunt for the last stranded grains.
        /// </summary>
        private int AbsorbRemaining(OutletSpan span) {
            int budget = _assistGrainsPerTick;
            int column = ((span.TankStartX + span.TankEndX) / 2) - span.InteriorOffsetX;
            int absorbed = 0;
            for (int y = 0; y < _tank.Height && absorbed < budget; y++) {
                for (int x = 0; x < _tank.Width && absorbed < budget; x++) {
                    ushort cell = _tank.GetCell(x, y);
                    if (!SandCell.IsGrain(cell) || !_rule.Accepts(_colorId, SandCell.GetColorId(cell))) {
                        continue;
                    }

                    _tank.Remove(x, y);
                    if (!_queue.TryEnqueue(cell, column)) {
                        _unqueuedGrains++;
                    }

                    absorbed++;
                }
            }

            return absorbed;
        }
    }
}