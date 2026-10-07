using UnityEngine;

namespace SandGameplay.Sand {
    /// <summary>Sand colors and tone rules. Builds the color lookup table used by <see cref="SandGridView"/>.</summary>
    [CreateAssetMenu(fileName = "SandPalette", menuName = "Sand Gameplay/Sand Palette")]
    public sealed class SandPalette : ScriptableObject {
        [SerializeField] private SandColorDefinition[] colors = CreateDefaultColors();
        [SerializeField, Range(1f, 2f)] private float sparkleBrightness = 1.25f;
        [SerializeField, Range(0f, 0.1f)] private float sparkleChance = 0.015f;
        [SerializeField, Range(0, 4)] private int toneJitter = 2;

        public int ColorCount {
            get {
                return colors.Length;
            }
        }

        public float SparkleChance {
            get {
                return sparkleChance;
            }
        }

        public int ToneJitter {
            get {
                return toneJitter;
            }
        }

        public bool IsValidColorId(int colorId) {
            return colorId >= 0 && colorId < colors.Length && colorId < SandCell.MaxColorCount;
        }

        public Color GetBodyColor(int colorId) {
            if (!IsValidColorId(colorId)) {
                Debug.LogError($"[SandPalette] Color id {colorId} is not defined in '{name}'.", this);
                return Color.magenta;
            }

            return colors[colorId].Body;
        }

        public Color GetSandColor(int colorId) {
            if (!IsValidColorId(colorId)) {
                Debug.LogError($"[SandPalette] Color id {colorId} is not defined in '{name}'.", this);
                return Color.magenta;
            }

            return colors[colorId].Base;
        }

        public int FindNearestColorId(Color32 color) {
            int bestId = 0;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < colors.Length; i++) {
                Color candidate = colors[i].Base;
                float red = candidate.r - (color.r / 255f);
                float green = candidate.g - (color.g / 255f);
                float blue = candidate.b - (color.b / 255f);
                float distance = (red * red) + (green * green) + (blue * blue);
                if (distance < bestDistance) {
                    bestDistance = distance;
                    bestId = i;
                }
            }

            return bestId;
        }

        /// <summary>Creates a grain of the given color whose tone is jittered (and occasionally a sparkle) for a natural look.</summary>
        public ushort CreateGrain(int colorId, int tone, ref XorShift32 random) {
            if (random.NextFloat01() < sparkleChance) {
                return SandCell.CreateGrain(colorId, SandCell.SparkleTone);
            }

            int jitteredTone = tone + random.NextInt(-toneJitter, toneJitter + 1);
            return SandCell.CreateGrain(colorId, Mathf.Clamp(jitteredTone, 0, SandCell.MaxRegularTone));
        }

        /// <summary>Builds the colorId x tone lookup table. Tones 0..14 ramp shadow → base → highlight; tone 15 is a sparkle.</summary>
        public Color32[] BuildToneLut() {
            Color32[] lut = new Color32[SandCell.LutSize];
            int count = Mathf.Min(colors.Length, SandCell.MaxColorCount);
            for (int colorId = 0; colorId < count; colorId++) {
                SandColorDefinition definition = colors[colorId];
                int rowStart = colorId * SandCell.ToneCount;
                for (int tone = 0; tone <= SandCell.MaxRegularTone; tone++) {
                    float t = tone / (float)SandCell.MaxRegularTone;
                    Color color = t < 0.5f
                        ? Color.Lerp(definition.Shadow, definition.Base, t * 2f)
                        : Color.Lerp(definition.Base, definition.Highlight, (t - 0.5f) * 2f);
                    color.a = 1f;
                    lut[rowStart + tone] = color;
                }

                Color sparkle = definition.Highlight * sparkleBrightness;
                sparkle.a = 1f;
                lut[rowStart + SandCell.SparkleTone] = sparkle;
            }

            return lut;
        }

        private void OnValidate() {
            if (colors.Length > SandCell.MaxColorCount) {
                Debug.LogWarning($"[SandPalette] '{name}' defines {colors.Length} colors; only the first {SandCell.MaxColorCount} can be encoded.", this);
            }
        }

        private static SandColorDefinition[] CreateDefaultColors() {
            return new SandColorDefinition[] {
                new SandColorDefinition("Yellow", new Color(0.86f, 0.45f, 0.06f), new Color(1f, 0.78f, 0.18f), new Color(1f, 0.96f, 0.66f), new Color(1f, 0.76f, 0.14f)),
                new SandColorDefinition("Red", new Color(0.60f, 0.07f, 0.12f), new Color(0.93f, 0.22f, 0.25f), new Color(1f, 0.58f, 0.55f), new Color(0.95f, 0.27f, 0.27f)),
                new SandColorDefinition("Blue", new Color(0.09f, 0.24f, 0.70f), new Color(0.22f, 0.50f, 0.98f), new Color(0.64f, 0.84f, 1f), new Color(0.24f, 0.52f, 0.98f)),
                new SandColorDefinition("Green", new Color(0.07f, 0.48f, 0.22f), new Color(0.22f, 0.82f, 0.38f), new Color(0.64f, 1f, 0.66f), new Color(0.25f, 0.80f, 0.36f)),
                new SandColorDefinition("Pink", new Color(0.68f, 0.17f, 0.54f), new Color(0.98f, 0.42f, 0.78f), new Color(1f, 0.76f, 0.93f), new Color(0.96f, 0.42f, 0.76f)),
                new SandColorDefinition("White", new Color(0.68f, 0.72f, 0.84f), new Color(0.92f, 0.94f, 0.98f), new Color(1f, 1f, 1f), new Color(0.92f, 0.94f, 0.98f))
            };
        }
    }
}