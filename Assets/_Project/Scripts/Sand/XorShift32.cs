namespace SandGameplay.Sand {
    /// <summary>Small, fast, deterministic random generator for the sand automaton. Store it in a non-readonly field.</summary>
    public struct XorShift32 {
        private const uint FallbackSeed = 0x9E3779B9u;

        private uint _state;

        public XorShift32(uint seed) {
            _state = seed == 0u ? FallbackSeed : seed;
        }

        public uint NextUInt() {
            uint x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        public bool NextBool() {
            return (NextUInt() & 0x80000000u) != 0u;
        }

        public float NextFloat01() {
            return (NextUInt() >> 8) * (1.0f / 16777216.0f);
        }

        public int NextInt(int minInclusive, int maxExclusive) {
            if (maxExclusive <= minInclusive) {
                return minInclusive;
            }

            uint range = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(NextUInt() % range);
        }
    }
}