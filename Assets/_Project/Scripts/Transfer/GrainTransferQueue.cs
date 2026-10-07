using System;

using SandGameplay.Sand;

namespace SandGameplay.Transfer {
    /// <summary>Preallocated FIFO ring buffer of grains travelling from the tank into the box. Never allocates after construction.</summary>
    public sealed class GrainTransferQueue {
        private readonly ushort[] _cells;
        private readonly short[] _interiorColumns;
        private int _head;

        public GrainTransferQueue(int capacity) {
            if (capacity <= 0) {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Queue capacity must be positive.");
            }

            _cells = new ushort[capacity];
            _interiorColumns = new short[capacity];
        }

        public int Capacity {
            get {
                return _cells.Length;
            }
        }

        public int Count { get; private set; }

        public bool TryEnqueue(ushort cell, int interiorColumn) {
            if (Count == _cells.Length) {
                return false;
            }

            int tail = (_head + Count) % _cells.Length;
            _cells[tail] = cell;
            _interiorColumns[tail] = (short)interiorColumn;
            Count++;
            return true;
        }

        public bool TryDequeue(out ushort cell, out int interiorColumn) {
            if (Count == 0) {
                cell = SandCell.Empty;
                interiorColumn = 0;
                return false;
            }

            cell = _cells[_head];
            interiorColumn = _interiorColumns[_head];
            _head = (_head + 1) % _cells.Length;
            Count--;
            return true;
        }

        public void Clear() {
            _head = 0;
            Count = 0;
        }
    }
}