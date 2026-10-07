using System;

namespace SandGameplay.Sand {
    /// <summary>
    /// Falling-sand cellular automaton on a fixed-size grid. Row 0 is the bottom row and everything outside the
    /// grid behaves as a solid wall. A grid that stops moving goes to sleep until it is woken by a change.
    /// </summary>
    public sealed class SandGrid {
        private readonly ushort[] _cells;
        private XorShift32 _random;
        private bool _scanLeftToRight = true;

        public SandGrid(int width, int height, uint seed) {
            if (width <= 0) {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Grid width must be positive.");
            }

            if (height <= 0) {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Grid height must be positive.");
            }

            Width = width;
            Height = height;
            _cells = new ushort[width * height];
            _random = new XorShift32(seed);
            IsSettled = true;
        }

        public int Width { get; }

        public int Height { get; }

        public int GrainCount { get; private set; }

        /// <summary>Increments whenever any cell changes; views compare it to decide whether to re-upload.</summary>
        public int Version { get; private set; }

        public bool IsSettled { get; private set; }

        public ReadOnlySpan<ushort> Cells {
            get {
                return _cells;
            }
        }

        public bool IsInside(int x, int y) {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public bool IsEmpty(int x, int y) {
            return IsInside(x, y) && _cells[(y * Width) + x] == SandCell.Empty;
        }

        public ushort GetCell(int x, int y) {
            ThrowIfOutside(x, y);
            return _cells[(y * Width) + x];
        }

        /// <summary>Overwrites a cell (used for pattern fills and top-ups). Non-grain values clear the cell.</summary>
        public void SetCell(int x, int y, ushort cell) {
            ThrowIfOutside(x, y);
            int index = (y * Width) + x;
            bool wasGrain = SandCell.IsGrain(_cells[index]);
            bool isGrain = SandCell.IsGrain(cell);
            _cells[index] = isGrain ? cell : SandCell.Empty;
            if (isGrain && !wasGrain) {
                GrainCount++;
            } else if (!isGrain && wasGrain) {
                GrainCount--;
            }

            MarkChanged();
        }

        public bool TrySpawn(int x, int y, ushort cell) {
            if (!SandCell.IsGrain(cell) || !IsEmpty(x, y)) {
                return false;
            }

            _cells[(y * Width) + x] = cell;
            GrainCount++;
            MarkChanged();
            return true;
        }

        /// <summary>Removes the grain at the cell and returns it, or returns <see cref="SandCell.Empty"/>.</summary>
        public ushort Remove(int x, int y) {
            if (!IsInside(x, y)) {
                return SandCell.Empty;
            }

            int index = (y * Width) + x;
            ushort cell = _cells[index];
            if (cell == SandCell.Empty) {
                return SandCell.Empty;
            }

            _cells[index] = SandCell.Empty;
            GrainCount--;
            MarkChanged();
            return cell;
        }

        public void Clear() {
            Array.Clear(_cells, 0, _cells.Length);
            GrainCount = 0;
            Version++;
            IsSettled = true;
        }

        public void Wake() {
            IsSettled = false;
        }

        public int CountColor(int colorId) {
            int count = 0;
            for (int i = 0; i < _cells.Length; i++) {
                ushort cell = _cells[i];
                if (SandCell.IsGrain(cell) && SandCell.GetColorId(cell) == colorId) {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// One automaton tick. Rows are processed bottom to top, so a grain can only move into a row that was
        /// already processed and therefore moves at most once per tick. The horizontal scan direction alternates
        /// every tick so piles do not lean to one side.
        /// </summary>
        public void Step(float slideChance) {
            if (IsSettled) {
                return;
            }

            int width = Width;
            int moves = 0;
            bool hasDeferredSlides = false;
            bool leftToRight = _scanLeftToRight;
            _scanLeftToRight = !_scanLeftToRight;

            for (int y = 1; y < Height; y++) {
                int rowStart = y * width;
                int belowRowStart = rowStart - width;
                for (int i = 0; i < width; i++) {
                    int x = leftToRight ? i : width - 1 - i;
                    int index = rowStart + x;
                    ushort cell = _cells[index];
                    if (cell == SandCell.Empty) {
                        continue;
                    }

                    int below = belowRowStart + x;
                    if (_cells[below] == SandCell.Empty) {
                        _cells[below] = cell;
                        _cells[index] = SandCell.Empty;
                        moves++;
                        continue;
                    }

                    bool canSlideLeft = x > 0 && _cells[below - 1] == SandCell.Empty;
                    bool canSlideRight = x < width - 1 && _cells[below + 1] == SandCell.Empty;
                    if (!canSlideLeft && !canSlideRight) {
                        continue;
                    }

                    if (slideChance < 1f && _random.NextFloat01() >= slideChance) {
                        hasDeferredSlides = true;
                        continue;
                    }

                    bool slideLeft = canSlideLeft && (!canSlideRight || _random.NextBool());
                    int target = slideLeft ? below - 1 : below + 1;
                    _cells[target] = cell;
                    _cells[index] = SandCell.Empty;
                    moves++;
                }
            }

            if (moves > 0) {
                Version++;
            } else if (!hasDeferredSlides) {
                IsSettled = true;
            }
        }

        private void MarkChanged() {
            Version++;
            IsSettled = false;
        }

        private void ThrowIfOutside(int x, int y) {
            if (!IsInside(x, y)) {
                throw new ArgumentOutOfRangeException(nameof(x), $"Cell ({x}, {y}) is outside the {Width}x{Height} grid.");
            }
        }
    }
}