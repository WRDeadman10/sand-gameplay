using System;

namespace SandGameplay.Simulation {
    /// <summary>
    /// Fixed-timestep accumulator with a per-frame cap. Time beyond the cap is dropped so a hitch cannot
    /// snowball into ever longer frames.
    /// </summary>
    public sealed class FixedTickClock {
        private readonly float _interval;
        private readonly int _maxTicksPerFrame;
        private float _accumulator;

        public FixedTickClock(int ticksPerSecond, int maxTicksPerFrame) {
            if (ticksPerSecond <= 0) {
                throw new ArgumentOutOfRangeException(nameof(ticksPerSecond), ticksPerSecond, "Tick rate must be positive.");
            }

            if (maxTicksPerFrame <= 0) {
                throw new ArgumentOutOfRangeException(nameof(maxTicksPerFrame), maxTicksPerFrame, "Tick cap must be positive.");
            }

            _interval = 1f / ticksPerSecond;
            _maxTicksPerFrame = maxTicksPerFrame;
        }

        public float Interval {
            get {
                return _interval;
            }
        }

        /// <summary>Adds the frame time and returns how many ticks to run this frame.</summary>
        public int Consume(float deltaTime) {
            if (deltaTime > 0f) {
                _accumulator += deltaTime;
            }

            int ticks = 0;
            while (_accumulator >= _interval && ticks < _maxTicksPerFrame) {
                _accumulator -= _interval;
                ticks++;
            }

            if (ticks == _maxTicksPerFrame && _accumulator >= _interval) {
                _accumulator = 0f;
            }

            return ticks;
        }

        public void Reset() {
            _accumulator = 0f;
        }
    }
}