using System;

namespace SandGameplay.Container {
    /// <summary>Fill accounting for one box: capacity, received grains, integer percent and a single completion event.</summary>
    public sealed class SandContainer {
        private readonly float _completionThreshold;
        private readonly float _assistThreshold;

        public SandContainer(float completionThreshold, float assistThreshold) {
            if (completionThreshold <= 0f || completionThreshold > 1f) {
                throw new ArgumentOutOfRangeException(nameof(completionThreshold), completionThreshold, "Completion threshold must be in (0, 1].");
            }

            _completionThreshold = completionThreshold;
            _assistThreshold = Math.Clamp(assistThreshold, 0f, completionThreshold);
        }

        /// <summary>Raised only when the integer percent changes.</summary>
        public event Action<int> PercentChanged;

        /// <summary>Raised exactly once per <see cref="Reset"/>.</summary>
        public event Action Completed;

        public int Capacity { get; private set; }

        public int Received { get; private set; }

        public int Percent { get; private set; }

        public bool IsComplete { get; private set; }

        public float FillRatio {
            get {
                return Capacity > 0 ? (float)Received / Capacity : 0f;
            }
        }

        /// <summary>True once the box is nearly full; the flow then absorbs the remaining acceptable grains.</summary>
        public bool IsAssistActive {
            get {
                return !IsComplete && Capacity > 0 && FillRatio >= _assistThreshold;
            }
        }

        public void Reset(int capacity) {
            if (capacity < 0) {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity cannot be negative.");
            }

            Capacity = capacity;
            Received = 0;
            Percent = 0;
            IsComplete = false;
            PercentChanged?.Invoke(0);
        }

        public void AddReceived(int count) {
            if (IsComplete || count <= 0 || Capacity <= 0) {
                return;
            }

            Received = Math.Min(Capacity, Received + count);
            int percent = (int)((100L * Received) / Capacity);
            if (FillRatio >= _completionThreshold) {
                percent = 100;
            }

            if (percent != Percent) {
                Percent = percent;
                PercentChanged?.Invoke(percent);
            }

            if (FillRatio >= _completionThreshold) {
                IsComplete = true;
                Completed?.Invoke();
            }
        }
    }
}