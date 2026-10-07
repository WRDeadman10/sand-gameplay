using SandGameplay.Container;
using SandGameplay.Simulation;

using UnityEngine;

namespace SandGameplay.Presentation {
    /// <summary>Sprays a few grain particles from the box rim while sand is actually flowing into it.</summary>
    public sealed class PourSprayVfx : MonoBehaviour {
        [SerializeField] private ParticleSystem spray;
        [SerializeField, Min(0f)] private float particlesPerGrain = 0.06f;
        [SerializeField, Min(0)] private int maxParticlesPerFrame = 4;

        private ContainerPiece _container;
        private SandSimulationRunner _runner;
        private Color _color;
        private float _accumulator;

        public void Bind(ContainerPiece container, SandSimulationRunner runner, Color color) {
            _container = container;
            _runner = runner;
            _color = color;
            _accumulator = 0f;
            if (spray == null) {
                Debug.LogError("[PourSprayVfx] No particle system assigned.", this);
            }
        }

        private void LateUpdate() {
            if (_container == null || _runner == null || spray == null) {
                return;
            }

            int transferred = _runner.TransferredThisFrame;
            bool isFlowing = _container.IsDocked && transferred > 0;
            if (Audio.AudioManager.Instance != null) {
                Audio.AudioManager.Instance.SetPouring(isFlowing);
            }

            if (!isFlowing) {
                _accumulator = 0f;
                return;
            }

            _accumulator += transferred * particlesPerGrain;
            int count = Mathf.Min(maxParticlesPerFrame, Mathf.FloorToInt(_accumulator));
            if (count <= 0) {
                return;
            }

            _accumulator -= count;
            Rect opening = _container.OpeningWorldRect;
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.startColor = _color;
            for (int i = 0; i < count; i++) {
                emitParams.position = new Vector3(Random.Range(opening.xMin, opening.xMax), opening.yMax, 0f);
                spray.Emit(emitParams, 1);
            }
        }
    }
}