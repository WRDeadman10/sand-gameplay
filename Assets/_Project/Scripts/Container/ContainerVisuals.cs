using UnityEngine;

namespace SandGameplay.Container {
    /// <summary>
    /// Box presentation: tinting and layout of the box sprites, selection lift and outline, dock bump, and the
    /// visual root that the completion sequence animates.
    /// </summary>
    public sealed class ContainerVisuals : MonoBehaviour {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer interiorRenderer;
        [SerializeField] private SpriteRenderer outlineRenderer;
        [SerializeField] private SpriteRenderer flashRenderer;
        [SerializeField] private Transform sandViewTransform;
        [SerializeField] private Transform labelTransform;
        [SerializeField, Range(0f, 1f)] private float interiorBrightness = 0.36f;
        [SerializeField, Min(0f)] private float outlinePadding = 0.07f;
        [SerializeField, Tooltip("Label centre, measured from the interior's bottom-left corner.")] private Vector2 labelOffset = new Vector2(0.3f, 0.14f);
        [SerializeField, Min(1f)] private float selectedScale = 1.06f;
        [SerializeField, Min(1f)] private float scaleSharpness = 18f;
        [SerializeField, Min(0f)] private float dockBumpStrength = 0.06f;
        [SerializeField, Min(0.01f)] private float dockBumpDuration = 0.2f;

        private Vector3 _restLocalPosition;
        private float _currentScale = 1f;
        private float _targetScale = 1f;
        private float _bumpElapsed = -1f;
        private bool _isAnimationLocked;

        public Transform VisualRoot {
            get {
                return visualRoot;
            }
        }

        public SpriteRenderer FlashRenderer {
            get {
                return flashRenderer;
            }
        }

        public Color BodyColor { get; private set; }

        public bool HasReferences() {
            return visualRoot != null && bodyRenderer != null && interiorRenderer != null && outlineRenderer != null && flashRenderer != null && sandViewTransform != null && labelTransform != null;
        }

        public void Configure(Vector2 footprintSize, Rect interiorLocalRect, Color bodyColor) {
            BodyColor = bodyColor;
            _restLocalPosition = new Vector3(footprintSize.x * 0.5f, footprintSize.y * 0.5f, 0f);
            bodyRenderer.size = footprintSize;
            bodyRenderer.color = bodyColor;
            interiorRenderer.size = interiorLocalRect.size;
            interiorRenderer.transform.localPosition = new Vector3(interiorLocalRect.center.x, interiorLocalRect.center.y, 0f);
            interiorRenderer.color = new Color(bodyColor.r * interiorBrightness, bodyColor.g * interiorBrightness, bodyColor.b * interiorBrightness, 1f);
            outlineRenderer.size = footprintSize + (Vector2.one * (outlinePadding * 2f));
            flashRenderer.size = footprintSize;
            sandViewTransform.localPosition = new Vector3(interiorLocalRect.xMin, interiorLocalRect.yMin, 0f);
            labelTransform.localPosition = new Vector3(interiorLocalRect.xMin + labelOffset.x, interiorLocalRect.yMin + labelOffset.y, 0f);
            ResetVisuals();
        }

        public void SetSelected(bool selected) {
            outlineRenderer.enabled = selected;
            _targetScale = selected ? selectedScale : 1f;
        }

        public void PlayDockBump() {
            _bumpElapsed = 0f;
        }

        /// <summary>Hands the visual root to an external animation (the completion sequence).</summary>
        public void LockForAnimation() {
            _isAnimationLocked = true;
            outlineRenderer.enabled = false;
            visualRoot.localScale = Vector3.one;
        }

        public void ResetVisuals() {
            _isAnimationLocked = false;
            _currentScale = 1f;
            _targetScale = 1f;
            _bumpElapsed = -1f;
            visualRoot.gameObject.SetActive(true);
            visualRoot.localPosition = _restLocalPosition;
            visualRoot.localRotation = Quaternion.identity;
            visualRoot.localScale = Vector3.one;
            outlineRenderer.enabled = false;
            Color flash = flashRenderer.color;
            flash.a = 0f;
            flashRenderer.color = flash;
        }

        private void Update() {
            if (_isAnimationLocked || visualRoot == null) {
                return;
            }

            float deltaTime = Time.deltaTime;
            _currentScale = Mathf.Lerp(_currentScale, _targetScale, 1f - Mathf.Exp(-scaleSharpness * deltaTime));
            float bump = 0f;
            if (_bumpElapsed >= 0f) {
                _bumpElapsed += deltaTime;
                float t = _bumpElapsed / dockBumpDuration;
                if (t >= 1f) {
                    _bumpElapsed = -1f;
                } else {
                    bump = Mathf.Sin(t * Mathf.PI) * dockBumpStrength;
                }
            }

            float scale = _currentScale + bump;
            visualRoot.localScale = new Vector3(scale, scale, 1f);
        }
    }
}