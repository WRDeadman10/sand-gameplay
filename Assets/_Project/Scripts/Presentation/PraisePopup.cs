using SandGameplay.Utilities;

using TMPro;

using UnityEngine;

namespace SandGameplay.Presentation {
    /// <summary>World-space praise word that pops in with overshoot, rises and fades out. Starts inactive in the scene.</summary>
    public sealed class PraisePopup : MonoBehaviour {
        [SerializeField] private TMP_Text label;
        [SerializeField] private AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.16f, 1.25f), new Keyframe(0.3f, 1f), new Keyframe(1f, 1f));
        [SerializeField] private AnimationCurve alphaCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.72f, 1f), new Keyframe(1f, 0f));
        [SerializeField, Min(0.05f)] private float duration = 1.15f;
        [SerializeField] private float riseDistance = 0.7f;
        [SerializeField, Min(0f)] private float tiltDegrees = 6f;

        private Vector3 _origin;
        private Color _color;
        private float _elapsed = -1f;

        public void Show(string word, Vector3 worldPosition, Color color) {
            if (label == null) {
                Debug.LogError($"[PraisePopup] '{name}' has no label assigned.", this);
                return;
            }

            gameObject.SetActive(true);
            label.text = word;
            _color = color;
            _origin = worldPosition;
            _elapsed = 0f;
            transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-tiltDegrees, tiltDegrees));
            Apply(0f);
        }

        public void Hide() {
            _elapsed = -1f;
            gameObject.SetActive(false);
        }

        private void Update() {
            if (_elapsed < 0f) {
                return;
            }

            _elapsed += Time.deltaTime;
            float t = _elapsed / duration;
            if (t >= 1f) {
                Hide();
                return;
            }

            Apply(t);
        }

        private void Apply(float t) {
            float scale = scaleCurve.Evaluate(t);
            transform.localScale = new Vector3(scale, scale, 1f);
            transform.position = _origin + (Vector3.up * (riseDistance * Easing.OutQuad(t)));
            Color color = _color;
            color.a = alphaCurve.Evaluate(t);
            label.color = color;
        }
    }
}