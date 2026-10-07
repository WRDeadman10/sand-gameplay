using SandGameplay.Container;

using Unity.Profiling;

using UnityEngine;

namespace SandGameplay.Simulation {
    /// <summary>Drives <see cref="SandFlowSimulation"/> at a fixed tick rate from the frame loop.</summary>
    [DefaultExecutionOrder(0)]
    public sealed class SandSimulationRunner : MonoBehaviour {
        private static readonly ProfilerMarker TickMarker = new ProfilerMarker("Sand.Tick");

        private SandFlowSimulation _flow;
        private ContainerPiece _container;
        private System.Collections.Generic.IReadOnlyList<ContainerPiece> _containers;
        private FixedTickClock _clock;
        private bool _isTransferEnabled;

        /// <summary>Grains that left the tank this frame; read by presentation effects in LateUpdate.</summary>
        public int TransferredThisFrame { get; private set; }

        public void Bind(SandFlowSimulation flow, System.Collections.Generic.IReadOnlyList<ContainerPiece> containers, FixedTickClock clock) {
            _flow = flow;
            _containers = containers;
            _container = containers != null && containers.Count > 0 ? containers[0] : null;
            _clock = clock;
            TransferredThisFrame = 0;
        }

        public void Bind(SandFlowSimulation flow, ContainerPiece container, FixedTickClock clock) {
            _flow = flow;
            _container = container;
            _containers = container != null ? new[] { container } : null;
            _clock = clock;
            TransferredThisFrame = 0;
        }

        public void SetTransferEnabled(bool transferEnabled) {
            _isTransferEnabled = transferEnabled;
        }

        public void ResetClock() {
            if (_clock != null) {
                _clock.Reset();
            }

            TransferredThisFrame = 0;
        }

        private void Update() {
            TransferredThisFrame = 0;
            if (_flow == null || _clock == null) {
                return;
            }

            int ticks = _clock.Consume(Time.deltaTime);
            for (int i = 0; i < ticks; i++) {
                using (TickMarker.Auto()) {
                    if (_containers != null) {
                        _flow.Tick(_containers, _isTransferEnabled);
                    } else if (_container != null) {
                        _flow.Tick(_container.IsDocked, _container.OutletSpan, _isTransferEnabled);
                    }
                }

                TransferredThisFrame += _flow.TransferredLastTick;
            }
        }
    }
}