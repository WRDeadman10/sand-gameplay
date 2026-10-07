using System;

using NUnit.Framework;

using SandGameplay.Sand;
using SandGameplay.Source;

using UnityEngine;

namespace SandGameplay.Tests {
    public sealed class TankFillPatternTests {
        private SandPalette _palette;
        private Texture2D _texture;

        [SetUp]
        public void SetUp() {
            _palette = ScriptableObject.CreateInstance<SandPalette>();
        }

        [TearDown]
        public void TearDown() {
            UnityEngine.Object.DestroyImmediate(_palette);
            if (_texture != null) {
                UnityEngine.Object.DestroyImmediate(_texture);
            }
        }

        [Test]
        public void Fill_Solid_FillsRowsUpToRatio_WithPatternColor() {
            TankFillPattern pattern = new TankFillPattern(TankFillMode.Solid, 2, 0.5f, 7, null, PixelArtColorMapping.ToneOfFillColor);
            SandGrid grid = new SandGrid(12, 10, 1u);

            pattern.Fill(grid, _palette, 42u);

            Assert.AreEqual(60, grid.GrainCount);
            Assert.AreEqual(60, grid.CountColor(2));
            Assert.IsFalse(grid.IsEmpty(5, 4));
            Assert.IsTrue(grid.IsEmpty(5, 5));
        }

        [Test]
        public void Fill_PixelArtTone_MapsLuminanceToTone_AndAlphaToEmpty() {
            _texture = CreateTexture(2, 2, new Color32[] {
                new Color32(0, 0, 0, 255), new Color32(255, 255, 255, 255),
                new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 0)
            });
            TankFillPattern pattern = new TankFillPattern(TankFillMode.PixelArt, 3, 1f, 7, _texture, PixelArtColorMapping.ToneOfFillColor);
            SandGrid grid = new SandGrid(8, 8, 1u);

            pattern.Fill(grid, _palette, 42u);

            Assert.AreEqual(32, grid.GrainCount);
            Assert.AreEqual(32, grid.CountColor(3));
            for (int y = 0; y < 4; y++) {
                for (int x = 0; x < 8; x++) {
                    int tone = SandCell.GetTone(grid.GetCell(x, y));
                    if (tone == SandCell.SparkleTone) {
                        continue;
                    }

                    if (x < 4) {
                        Assert.LessOrEqual(tone, _palette.ToneJitter);
                    } else {
                        Assert.GreaterOrEqual(tone, SandCell.MaxRegularTone - _palette.ToneJitter);
                    }
                }
            }

            Assert.IsTrue(grid.IsEmpty(0, 4));
            Assert.IsTrue(grid.IsEmpty(7, 7));
        }

        [Test]
        public void Fill_PixelArtPaletteColor_PicksNearestPaletteColor() {
            _texture = CreateTexture(1, 1, new Color32[] { new Color32(236, 56, 64, 255) });
            TankFillPattern pattern = new TankFillPattern(TankFillMode.PixelArt, 0, 1f, 7, _texture, PixelArtColorMapping.NearestPaletteColor);
            SandGrid grid = new SandGrid(4, 4, 1u);

            pattern.Fill(grid, _palette, 42u);

            Assert.AreEqual(16, grid.CountColor(1));
        }

        [Test]
        public void Fill_SameSeed_IsDeterministic() {
            TankFillPattern pattern = new TankFillPattern(TankFillMode.Solid, 0, 0.8f, 7, null, PixelArtColorMapping.ToneOfFillColor);
            SandGrid first = new SandGrid(20, 10, 1u);
            SandGrid second = new SandGrid(20, 10, 1u);

            pattern.Fill(first, _palette, 99u);
            pattern.Fill(second, _palette, 99u);

            Assert.IsTrue(first.Cells.SequenceEqual(second.Cells));
        }

        [Test]
        public void Fill_NullGrid_Throws() {
            Assert.Throws<ArgumentNullException>(FillNullGrid);
        }

        private void FillNullGrid() {
            new TankFillPattern().Fill(null, _palette, 1u);
        }

        private static Texture2D CreateTexture(int width, int height, Color32[] pixels) {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }
    }
}