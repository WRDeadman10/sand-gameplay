using SandGameplay.Sand;

using UnityEngine;

namespace SandGameplay.Source {
    /// <summary>
    /// The sand tank: owns its grid, view and frame art. The transform sits at the tank interior's bottom-left corner.
    /// </summary>
    public sealed class SandSource : MonoBehaviour {
        [SerializeField] private SandGridView gridView;
        [SerializeField] private SpriteRenderer backRenderer;
        [SerializeField] private SpriteRenderer frameRenderer;
        [SerializeField] private SpriteRenderer lipRenderer;
        [SerializeField] private SpriteRenderer hintRenderer;
        [SerializeField, Min(0f)] private float framePadding = 0.1f;
        [SerializeField, Min(0f)] private float lipHeight = 0.26f;
        [SerializeField, Min(0f)] private float hintPadding = 0.2f;
        [SerializeField, Min(0.1f)] private float hintPulseSpeed = 4f;
        [SerializeField, Range(0f, 1f)] private float hintMinAlpha = 0.15f;
        [SerializeField, Range(0f, 1f)] private float hintMaxAlpha = 0.85f;

        private TankFillPattern _pattern;
        private SandPalette _palette;
        private uint _seed;
        private bool _isHintVisible;
        private float _hintAlpha;

        public SandGrid Grid { get; private set; }

        public Rect InteriorWorldRect { get; private set; }

        public bool Initialize(Rect interiorWorldRect, int grainsPerCell, TankFillPattern pattern, SandPalette palette, Color32[] toneLut, uint seed) {
            if (!HasReferences()) {
                return false;
            }

            if (pattern == null || palette == null) {
                Debug.LogError("[SandSource] A fill pattern and a palette are required.", this);
                return false;
            }

            _pattern = pattern;
            _palette = palette;
            _seed = seed;
            InteriorWorldRect = interiorWorldRect;
            int width = Mathf.RoundToInt(interiorWorldRect.width * grainsPerCell);
            int height = Mathf.RoundToInt(interiorWorldRect.height * grainsPerCell);
            Grid = new SandGrid(width, height, seed);
            transform.position = new Vector3(interiorWorldRect.xMin, interiorWorldRect.yMin, 0f);
            gridView.Bind(Grid, toneLut, grainsPerCell);
            LayoutArt();
            ResetFill();
            return true;
        }

        public void ResetFill() {
            _pattern.Fill(Grid, _palette, _seed);
        }

        public void SetHintVisible(bool visible) {
            _isHintVisible = visible;
        }

        private void Update() {
            if (hintRenderer == null) {
                return;
            }

            float target = 0f;
            if (_isHintVisible) {
                float wave = 0.5f + (0.5f * Mathf.Sin(Time.time * hintPulseSpeed));
                target = Mathf.Lerp(hintMinAlpha, hintMaxAlpha, wave);
            }

            _hintAlpha = Mathf.MoveTowards(_hintAlpha, target, Time.deltaTime * 4f);
            Color color = hintRenderer.color;
            color.a = _hintAlpha;
            hintRenderer.color = color;
            hintRenderer.enabled = _hintAlpha > 0.001f;
        }

        private void LayoutArt() {
            Vector2 size = InteriorWorldRect.size;
            Vector3 center = new Vector3(size.x * 0.5f, size.y * 0.5f, 0f);
            backRenderer.size = size;
            backRenderer.transform.localPosition = center;
            frameRenderer.size = size + (Vector2.one * (framePadding * 2f));
            frameRenderer.transform.localPosition = center;
            lipRenderer.size = new Vector2(size.x + (framePadding * 2f), lipHeight);
            lipRenderer.transform.localPosition = new Vector3(size.x * 0.5f, -lipHeight * 0.5f, 0f);
            hintRenderer.size = size + (Vector2.one * (hintPadding * 2f));
            hintRenderer.transform.localPosition = center;
        }

        private bool HasReferences() {
            if (gridView == null || backRenderer == null || frameRenderer == null || lipRenderer == null || hintRenderer == null) {
                Debug.LogError("[SandSource] Missing renderer references; check the Tank prefab.", this);
                return false;
            }

            return true;
        }
    }
}