using System;

using SandGameplay.Sand;

using UnityEngine;

namespace SandGameplay.Source {
    /// <summary>Describes the tank's initial sand. Filling is deterministic for a given seed, so restarts are identical.</summary>
    [Serializable]
    public sealed class TankFillPattern {
        private const byte MinimumOpaqueAlpha = 128;

        [SerializeField] private TankFillMode mode = TankFillMode.PixelArt;
        [SerializeField, Min(0)] private int colorId;
        [SerializeField, Range(0f, 1f)] private float solidFillRatio = 0.75f;
        [SerializeField, Range(0, SandCell.MaxRegularTone)] private int solidTone = 7;
        [SerializeField, Tooltip("Read/Write enabled, point filtered, no NPOT scaling. Alpha below 50% stays empty.")] private Texture2D pixelArt;
        [SerializeField] private PixelArtColorMapping pixelArtMapping = PixelArtColorMapping.ToneOfFillColor;

        public TankFillPattern() {
        }

        public TankFillPattern(TankFillMode fillMode, int fillColorId, float fillRatio, int fillTone, Texture2D pattern, PixelArtColorMapping mapping) {
            mode = fillMode;
            colorId = fillColorId;
            solidFillRatio = Mathf.Clamp01(fillRatio);
            solidTone = Mathf.Clamp(fillTone, 0, SandCell.MaxRegularTone);
            pixelArt = pattern;
            pixelArtMapping = mapping;
        }

        public TankFillMode Mode {
            get {
                return mode;
            }
        }

        public int ColorId {
            get {
                return colorId;
            }
        }

        public void Fill(SandGrid grid, SandPalette palette, uint seed) {
            if (grid == null) {
                throw new ArgumentNullException(nameof(grid));
            }

            if (palette == null) {
                throw new ArgumentNullException(nameof(palette));
            }

            grid.Clear();
            XorShift32 random = new XorShift32(seed);
            if (mode == TankFillMode.PixelArt) {
                if (pixelArt == null) {
                    Debug.LogError("[TankFillPattern] PixelArt mode has no texture assigned; falling back to a solid fill.");
                } else if (!pixelArt.isReadable) {
                    Debug.LogError($"[TankFillPattern] Texture '{pixelArt.name}' must have Read/Write enabled; falling back to a solid fill.");
                } else {
                    FillFromPixelArt(grid, palette, ref random);
                    grid.Wake();
                    return;
                }
            }

            FillSolid(grid, palette, ref random);
            grid.Wake();
        }

        private void FillSolid(SandGrid grid, SandPalette palette, ref XorShift32 random) {
            int rows = Mathf.Clamp(Mathf.RoundToInt(grid.Height * solidFillRatio), 0, grid.Height);
            for (int y = 0; y < rows; y++) {
                for (int x = 0; x < grid.Width; x++) {
                    grid.SetCell(x, y, palette.CreateGrain(colorId, solidTone, ref random));
                }
            }
        }

        private void FillFromPixelArt(SandGrid grid, SandPalette palette, ref XorShift32 random) {
            Color32[] pixels = pixelArt.GetPixels32();
            int textureWidth = pixelArt.width;
            int textureHeight = pixelArt.height;
            int middleTone = SandCell.MaxRegularTone / 2;
            for (int y = 0; y < grid.Height; y++) {
                int pixelY = Mathf.Min(textureHeight - 1, (y * textureHeight) / grid.Height);
                for (int x = 0; x < grid.Width; x++) {
                    int pixelX = Mathf.Min(textureWidth - 1, (x * textureWidth) / grid.Width);
                    Color32 pixel = pixels[(pixelY * textureWidth) + pixelX];
                    if (pixel.a < MinimumOpaqueAlpha) {
                        continue;
                    }

                    int grainColorId;
                    int tone;
                    if (pixelArtMapping == PixelArtColorMapping.NearestPaletteColor) {
                        grainColorId = palette.FindNearestColorId(pixel);
                        tone = middleTone;
                    } else {
                        grainColorId = colorId;
                        tone = Mathf.RoundToInt(Luminance(pixel) * SandCell.MaxRegularTone);
                    }

                    grid.SetCell(x, y, palette.CreateGrain(grainColorId, tone, ref random));
                }
            }
        }

        private static float Luminance(Color32 pixel) {
            return ((0.299f * pixel.r) + (0.587f * pixel.g) + (0.114f * pixel.b)) / 255f;
        }
    }
}