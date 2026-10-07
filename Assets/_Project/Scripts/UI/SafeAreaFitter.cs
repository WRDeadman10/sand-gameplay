using UnityEngine;

namespace SandGameplay.UI {
    /// <summary>Keeps a full-screen RectTransform inside <see cref="Screen.safeArea"/> (notches, rounded corners).</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour {
        private RectTransform _rectTransform;
        private Rect _appliedSafeArea;
        private Vector2Int _appliedScreenSize;

        private void Awake() {
            _rectTransform = GetComponent<RectTransform>();
            Apply();
        }

        private void Update() {
            if (Screen.safeArea != _appliedSafeArea || Screen.width != _appliedScreenSize.x || Screen.height != _appliedScreenSize.y) {
                Apply();
            }
        }

        private void Apply() {
            Rect safeArea = Screen.safeArea;
            _appliedSafeArea = safeArea;
            _appliedScreenSize = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0) {
                return;
            }

            Vector2 anchorMin = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
            Vector2 anchorMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }
    }
}