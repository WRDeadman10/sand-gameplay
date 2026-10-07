using UnityEngine;

namespace SandGameplay.Presentation {
    /// <summary>
    /// Fits the orthographic camera to the gameplay content on any aspect ratio (re-fits when the screen size
    /// changes) and scales the background sprite to cover the whole view.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFitter : MonoBehaviour {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private SpriteRenderer background;
        [SerializeField, Min(0f)] private float horizontalMargin = 0.4f;
        [SerializeField, Min(0f), Tooltip("Space above the content for the HUD, in world units.")] private float topMargin = 1.8f;
        [SerializeField, Min(0f), Tooltip("Space below the content for the hint, in world units.")] private float bottomMargin = 1.6f;

        private Rect _content;
        private bool _hasContent;
        private int _lastScreenWidth;
        private int _lastScreenHeight;

        public void Fit(Rect content) {
            _content = content;
            _hasContent = true;
            Apply();
        }

        private void LateUpdate() {
            if (_hasContent && (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)) {
                Apply();
            }
        }

        private void Apply() {
            if (targetCamera == null) {
                Debug.LogError("[CameraFitter] No camera assigned.", this);
                return;
            }

            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
            float aspect = Mathf.Max(0.01f, targetCamera.aspect);
            float halfWidth = (_content.width * 0.5f) + horizontalMargin;
            float halfHeight = (_content.height + topMargin + bottomMargin) * 0.5f;
            float size = Mathf.Max(halfHeight, halfWidth / aspect);
            targetCamera.orthographicSize = size;
            float centerY = _content.center.y + ((topMargin - bottomMargin) * 0.5f);
            transform.position = new Vector3(_content.center.x, centerY, transform.position.z);

            if (background == null || background.sprite == null) {
                return;
            }

            Vector2 spriteSize = background.sprite.bounds.size;
            float viewHeight = size * 2f;
            float viewWidth = viewHeight * aspect;
            float scale = Mathf.Max(viewWidth / spriteSize.x, viewHeight / spriteSize.y) * 1.02f;
            background.transform.position = new Vector3(_content.center.x, centerY, 0f);
            background.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}