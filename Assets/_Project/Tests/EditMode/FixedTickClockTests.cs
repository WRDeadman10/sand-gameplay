using NUnit.Framework;

using SandGameplay.Simulation;

namespace SandGameplay.Tests {
    public sealed class FixedTickClockTests {
        [Test]
        public void Consume_AccumulatesPartialFrames() {
            FixedTickClock clock = new FixedTickClock(100, 4);

            int first = clock.Consume(0.006f);
            int second = clock.Consume(0.006f);

            Assert.AreEqual(0, first);
            Assert.AreEqual(1, second);
        }

        [Test]
        public void Consume_LongFrame_IsCapped_AndExcessIsDropped() {
            FixedTickClock clock = new FixedTickClock(100, 4);

            int hitch = clock.Consume(1f);
            int next = clock.Consume(0f);

            Assert.AreEqual(4, hitch);
            Assert.AreEqual(0, next);
        }

        [Test]
        public void Reset_ClearsAccumulatedTime() {
            FixedTickClock clock = new FixedTickClock(100, 4);
            clock.Consume(0.009f);

            clock.Reset();

            Assert.AreEqual(0, clock.Consume(0.002f));
        }

        [Test]
        public void Consume_NegativeDelta_IsIgnored() {
            FixedTickClock clock = new FixedTickClock(100, 4);

            Assert.AreEqual(0, clock.Consume(-5f));
            Assert.AreEqual(1, clock.Consume(0.011f));
        }
    }
}