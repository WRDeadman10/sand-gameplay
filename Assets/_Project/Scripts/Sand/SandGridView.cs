using System;

using Unity.Collections;
using Unity.Profiling;

using UnityEngine;

namespace SandGameplay.Sand {
    /// <summary>
    /// Renders one <see cref="SandGrid"/> through a point-filtered texture on a sprite (pivot bottom-left,
    /// one grain = 1 / pixelsPerUnit world units). The texture is uploaded only when the grid changed.
    /// </summary>
    public sealed class SandGridView : MonoBehaviour {
        private static readonly ProfilerMarker UploadMarker = new ProfilerMarker("Sand.Upload");
        private static readonly Color32 EmptyPixel = new Color32(0, 0, 0, 0);

        [SerializeField] private SpriteRenderer spriteRenderer;

        private SandGrid _grid;
        private Color32[] _toneLut;
        private Texture2D _texture;
        private Sprite _sprite;
        private int _uploadedVersion = -1;

        public void Bind(SandGrid grid, Color32[] toneLut, int pixelsPerUnit) {
            if (grid == null) {
                throw new ArgumentNullException(nameof(grid));
            }

            if (toneLut == null || toneLut.Length < SandCell.LutSize) {
                throw new ArgumentException("The tone lookup table is missing or too small.", nameof(toneLut));
            }

            if (spriteRenderer == null) {
                Debug.LogError($"[SandGridView] '{name}' has no SpriteRenderer assigned.", this);
                return;
            }

            ReleaseTexture();
            _grid = grid;
            _toneLut = toneLut;
            _texture = new Texture2D(grid.Width, grid.Height, TextureFormat.RGBA32, false);
            _texture.name = $"{name}_SandTexture";
            _texture.filterMode = FilterMode.Point;
            _texture.wrapMode = TextureWrapMode.Clamp;
            _sprite = Sprite.Create(_texture, new Rect(0f, 0f, grid.Width, grid.Height), Vector2.zero, pixelsPerUnit, 0, SpriteMeshType.FullRect);
            _sprite.name = $"{name}_SandSprite";
            spriteRenderer.sprite = _sprite;
            Upload();
        }

        private void LateUpdate() {
            if (_grid != null && _texture != null && _grid.Version != _uploadedVersion) {
                Upload();
            }
        }

        private void OnDestroy() {
            ReleaseTexture();
        }

        private void Upload() {
            using (UploadMarker.Auto()) {
                NativeArray<Color32> pixels = _texture.GetPixelData<Color32>(0);
                ReadOnlySpan<ushort> cells = _grid.Cells;
                for (int i = 0; i < cells.Length; i++) {
                    ushort cell = cells[i];
                    pixels[i] = cell == SandCell.Empty ? EmptyPixel : _toneLut[SandCell.GetLutIndex(cell)];
                }

                _texture.Apply(false, false);
            }

            _uploadedVersion = _grid.Version;
        }

        private void ReleaseTexture() {
            if (_sprite != null) {
                Destroy(_sprite);
                _sprite = null;
            }

            if (_texture != null) {
                Destroy(_texture);
                _texture = null;
            }
        }
    }
}