using System.Globalization;

using TMPro;

using UnityEngine;

namespace SandGameplay.Container {
    /// <summary>Shows the box fill percentage from cached strings (no per-update allocation) and pulses on every 10% step.</summary>
    public sealed class ContainerLabelView : MonoBehaviour {
        private static readonly string[] PercentLabels = BuildPercentLabels();

        [SerializeField] private TMP_Text label;
        [SerializeField] private Transform pulseTarget;
        [SerializeField, Min(0f)] private float pulseStrength = 0.25f;
        [SerializeField, Min(0.01f)] private float pulseDuration = 0.22f;

        private SandContainer _container;
        private int _lastDecile;
        private float _pulseElapsed = -1f;

        public void Bind(SandContainer container) {
            Unbind();
            _container = container;
            if (_container == null) {
                return;
            }

            _container.PercentChanged += HandlePercentChanged;
            _lastDecile = _container.Percent / 10;
            SetText(_container.Percent);
        }

        private void Awake() {
            if (label == null) {
                Debug.LogError($"[ContainerLabelView] '{name}' has no label assigned.", this);
                enabled = false;
            }
        }

        private void OnDestroy() {
            Unbind();
        }

        private void Update() {
            if (_pulseElapsed < 0f || pulseTarget == null) {
                return;
            }

            _pulseElapsed += Time.deltaTime;
            float t = _pulseElapsed / pulseDuration;
            if (t >= 1f) {
                _pulseElapsed = -1f;
                pulseTarget.localScale = Vector3.one;
                return;
            }

            float scale = 1f + (Mathf.Sin(t * Mathf.PI) * pulseStrength);
            pulseTarget.localScale = new Vector3(scale, scale, 1f);
        }

        private void HandlePercentChanged(int percent) {
            SetText(percent);
            int decile = percent / 10;
            if (decile != _lastDecile) {
                _lastDecile = decile;
                if (percent > 0) {
                    _pulseElapsed = 0f;
                }
            }
        }

        private void SetText(int percent) {
            if (label != null) {
                label.text = PercentLabels[Mathf.Clamp(percent, 0, 100)];
            }
        }

        private void Unbind() {
            if (_container == null) {
                return;
            }

            _container.PercentChanged -= HandlePercentChanged;
            _container = null;
        }

        private static string[] BuildPercentLabels() {
            string[] labels = new string[101];
            for (int i = 0; i < labels.Length; i++) {
                labels[i] = i.ToString(CultureInfo.InvariantCulture) + "%";
            }

            return labels;
        }
    }
}