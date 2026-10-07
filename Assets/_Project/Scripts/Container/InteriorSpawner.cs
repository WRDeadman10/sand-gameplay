using System;

using SandGameplay.Sand;

namespace SandGameplay.Container {
    /// <summary>
    /// Turns logical grains (counted against the box capacity) into visual grains spawned in the top row of the
    /// box interior. The interior has fewer cells than the tank holds grains, so spawning follows a ratio and the
    /// grains keep their color and tone. Rows at or above <see cref="FillRows"/> are headroom under the rim.
    /// </summary>
    public sealed class InteriorSpawner {
        private static readonly int[] SpawnOffsets = new int[] { 0, -1, 1, -2, 2, -3, 3 };

        private readonly SandGrid _interior;
        private readonly int _fillRows;
        private readonly int _spreadRadius;
        private XorShift32 _random;
        private float _visualPerGrain;
        private float _accumulator;

        public InteriorSpawner(SandGrid interior, int fillRows, int spreadRadius, uint seed) {
            if (interior == null) {
                throw new ArgumentNullException(nameof(interior));
            }

            if (fillRows <= 0 || fillRows > interior.Height) {
                throw new ArgumentOutOfRangeException(nameof(fillRows), fillRows, "Fill rows must be within the interior height.");
            }

            _interior = interior;
            _fillRows = fillRows;
            _spreadRadius = Math.Max(0, spreadRadius);
            _random = new XorShift32(seed);
        }

        public int FillRows {
            get {
                return _fillRows;
            }
        }

        public int VisualCapacity {
            get {
                return _interior.Width * _fillRows;
            }
        }

        public void Reset(int logicalCapacity) {
            _accumulator = 0f;
            _visualPerGrain = logicalCapacity > 0 ? (float)VisualCapacity / logicalCapacity : 0f;
        }

        public void Spawn(ushort grain, int interiorColumn) {
            _accumulator += _visualPerGrain;
            while (_accumulator >= 1f) {
                _accumulator -= 1f;
                int column = interiorColumn + _random.NextInt(-_spreadRadius, _spreadRadius + 1);
                TrySpawnNear(grain, Math.Clamp(column, 0, _interior.Width - 1));
            }
        }

        /// <summary>Fills every empty interior cell and clears the headroom so a completed box reads as exactly full.</summary>
        public void FillToTop(SandPalette palette, int colorId, int tone) {
            if (palette == null) {
                throw new ArgumentNullException(nameof(palette));
            }

            for (int y = 0; y < _fillRows; y++) {
                for (int x = 0; x < _interior.Width; x++) {
                    if (_interior.IsEmpty(x, y)) {
                        _interior.SetCell(x, y, palette.CreateGrain(colorId, tone, ref _random));
                    }
                }
            }

            for (int y = _fillRows; y < _interior.Height; y++) {
                for (int x = 0; x < _interior.Width; x++) {
                    _interior.Remove(x, y);
                }
            }
        }

        private void TrySpawnNear(ushort grain, int column) {
            int top = _interior.Height - 1;
            for (int i = 0; i < SpawnOffsets.Length; i++) {
                if (_interior.TrySpawn(column + SpawnOffsets[i], top, grain)) {
                    return;
                }
            }
        }
    }
}