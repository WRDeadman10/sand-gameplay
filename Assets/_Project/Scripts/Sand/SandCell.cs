using System;

namespace SandGameplay.Sand {
    /// <summary>
    /// Encodes one grain in a <see cref="ushort"/>. Bit 0 marks an occupied cell; bits 1-10 hold the
    /// tone lookup index (<c>colorId * ToneCount + tone</c>). Zero is an empty cell.
    /// </summary>
    public static class SandCell {
        public const ushort Empty = 0;
        public const int ToneCount = 16;
        public const int MaxColorCount = 64;
        public const int LutSize = MaxColorCount * ToneCount;
        public const int SparkleTone = ToneCount - 1;
        public const int MaxRegularTone = ToneCount - 2;

        private const int OccupiedFlag = 1;
        private const int LutShift = 1;
        private const int LutMask = LutSize - 1;
        private const int ToneMask = ToneCount - 1;
        private const int ColorShift = 4;

        public static ushort CreateGrain(int colorId, int tone) {
            int safeColorId = Math.Clamp(colorId, 0, MaxColorCount - 1);
            int safeTone = Math.Clamp(tone, 0, ToneMask);
            int lutIndex = (safeColorId << ColorShift) | safeTone;
            return (ushort)(OccupiedFlag | (lutIndex << LutShift));
        }

        public static bool IsGrain(ushort cell) {
            return (cell & OccupiedFlag) != 0;
        }

        public static int GetLutIndex(ushort cell) {
            return (cell >> LutShift) & LutMask;
        }

        public static int GetColorId(ushort cell) {
            return GetLutIndex(cell) >> ColorShift;
        }

        public static int GetTone(ushort cell) {
            return GetLutIndex(cell) & ToneMask;
        }
    }
}