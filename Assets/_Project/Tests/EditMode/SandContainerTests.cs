using System.Collections.Generic;

using NUnit.Framework;

using SandGameplay.Container;

namespace SandGameplay.Tests {
    public sealed class SandContainerTests {
        private readonly List<int> _percentEvents = new List<int>();
        private int _completedEvents;

        [SetUp]
        public void SetUp() {
            _percentEvents.Clear();
            _completedEvents = 0;
        }

        [Test]
        public void AddReceived_RaisesPercentOnlyWhenIntegerPercentChanges() {
            SandContainer container = CreateContainer(1f, 0.97f, 1000);

            container.AddReceived(5);
            container.AddReceived(5);
            container.AddReceived(5);

            CollectionAssert.AreEqual(new int[] { 0, 1 }, _percentEvents);
            Assert.AreEqual(1, container.Percent);
        }

        [Test]
        public void AddReceived_ReachingCapacity_CompletesExactlyOnce() {
            SandContainer container = CreateContainer(1f, 0.97f, 100);

            container.AddReceived(60);
            container.AddReceived(60);
            container.AddReceived(10);

            Assert.IsTrue(container.IsComplete);
            Assert.AreEqual(100, container.Received);
            Assert.AreEqual(100, container.Percent);
            Assert.AreEqual(1, _completedEvents);
        }

        [Test]
        public void AddReceived_LowerCompletionThreshold_CompletesAndReportsHundredPercent() {
            SandContainer container = CreateContainer(0.9f, 0.85f, 100);

            container.AddReceived(90);

            Assert.IsTrue(container.IsComplete);
            Assert.AreEqual(100, container.Percent);
            Assert.AreEqual(1, _completedEvents);
        }

        [Test]
        public void IsAssistActive_OnlyBetweenAssistThresholdAndCompletion() {
            SandContainer container = CreateContainer(1f, 0.97f, 100);

            container.AddReceived(96);
            bool beforeThreshold = container.IsAssistActive;
            container.AddReceived(1);
            bool atThreshold = container.IsAssistActive;
            container.AddReceived(3);

            Assert.IsFalse(beforeThreshold);
            Assert.IsTrue(atThreshold);
            Assert.IsFalse(container.IsAssistActive);
        }

        [Test]
        public void Reset_ClearsProgress_AndAllowsAnotherCompletion() {
            SandContainer container = CreateContainer(1f, 0.97f, 10);
            container.AddReceived(10);

            container.Reset(20);
            container.AddReceived(20);

            Assert.AreEqual(20, container.Capacity);
            Assert.IsTrue(container.IsComplete);
            Assert.AreEqual(2, _completedEvents);
        }

        [Test]
        public void AddReceived_ZeroCapacity_NeverCompletes() {
            SandContainer container = CreateContainer(1f, 0.97f, 0);

            container.AddReceived(5);

            Assert.IsFalse(container.IsComplete);
            Assert.AreEqual(0, _completedEvents);
        }

        private SandContainer CreateContainer(float completionThreshold, float assistThreshold, int capacity) {
            SandContainer container = new SandContainer(completionThreshold, assistThreshold);
            container.PercentChanged += RecordPercent;
            container.Completed += RecordCompleted;
            container.Reset(capacity);
            return container;
        }

        private void RecordPercent(int percent) {
            _percentEvents.Add(percent);
        }

        private void RecordCompleted() {
            _completedEvents++;
        }
    }
}