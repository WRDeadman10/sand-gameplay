using NUnit.Framework;

using SandGameplay.Container;
using SandGameplay.Sand;
using SandGameplay.Simulation;
using SandGameplay.Transfer;

namespace SandGameplay.Tests {
    public sealed class SandFlowSimulationTests {
        private const int Yellow = 0;
        private const int Red = 1;

        private SandGrid _tank;
        private SandGrid _interior;
        private SandContainer _container;
        private InteriorSpawner _spawner;
        private int _completedEvents;

        [SetUp]
        public void SetUp() {
            _tank = new SandGrid(8, 6, 11u);
            _interior = new SandGrid(8, 9, 13u);
            _container = new SandContainer(1f, 0.97f);
            _spawner = new InteriorSpawner(_interior, 8, 1, 17u);
            _completedEvents = 0;
            _container.Completed += RecordCompleted;
        }

        [Test]
        public void Tick_DockedUnderSingleColorTank_FillsBoxCompletely() {
            FillTank(Yellow, 4);
            SandFlowSimulation flow = CreateFlow(ContainerColorRule.MatchColor);

            RunUntilComplete(flow, new OutletSpan(0, 8, 0), 2000);

            Assert.IsTrue(_container.IsComplete);
            Assert.AreEqual(1, _completedEvents);
            Assert.AreEqual(0, _tank.CountColor(Yellow));
            Assert.Greater(_interior.GrainCount, 0);
        }

        [Test]
        public void Tick_NotDocked_TransfersNothing() {
            FillTank(Yellow, 4);
            SandFlowSimulation flow = CreateFlow(ContainerColorRule.MatchColor);

            for (int i = 0; i < 100; i++) {
                flow.Tick(false, new OutletSpan(0, 8, 0), true);
            }

            Assert.AreEqual(0, _container.Received);
            Assert.AreEqual(32, _tank.GrainCount);
        }

        [Test]
        public void Tick_TransferDisabled_TransfersNothing() {
            FillTank(Yellow, 4);
            SandFlowSimulation flow = CreateFlow(ContainerColorRule.MatchColor);

            for (int i = 0; i < 100; i++) {
                flow.Tick(true, new OutletSpan(0, 8, 0), false);
            }

            Assert.AreEqual(0, _container.Received);
        }

        [Test]
        public void Tick_NarrowSpan_AssistAbsorbsStrandedGrains() {
            _container = new SandContainer(1f, 0.5f);
            _container.Completed += RecordCompleted;
            FillTank(Yellow, 3);
            SandFlowSimulation flow = CreateFlow(ContainerColorRule.MatchColor);

            RunUntilComplete(flow, new OutletSpan(3, 5, 0), 4000);

            Assert.IsTrue(_container.IsComplete);
            Assert.AreEqual(0, _tank.GrainCount);
        }

        [Test]
        public void Tick_MatchColor_NeverTakesOtherColors() {
            FillTank(Yellow, 2);
            for (int x = 0; x < _tank.Width; x++) {
                _tank.SetCell(x, 3, SandCell.CreateGrain(Red, 7));
            }

            SandFlowSimulation flow = CreateFlow(ContainerColorRule.MatchColor);

            RunUntilComplete(flow, new OutletSpan(0, 8, 0), 2000);

            Assert.IsTrue(_container.IsComplete);
            Assert.AreEqual(8, _tank.CountColor(Red));
        }

        [Test]
        public void Reset_ClearsPendingQueue() {
            FillTank(Yellow, 4);
            GrainTransferQueue queue = new GrainTransferQueue(64);
            SandFlowSimulation flow = new SandFlowSimulation(_tank, _interior, _container, _spawner, queue, ContainerColorRule.MatchColor, Yellow, 1f, 0, 8);
            queue.TryEnqueue(SandCell.CreateGrain(Yellow, 7), 0);

            flow.Reset();

            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual(0, flow.TransferredLastTick);
        }

        private SandFlowSimulation CreateFlow(ContainerColorRule rule) {
            int capacity = rule.CountAcceptable(_tank, Yellow);
            _container.Reset(capacity);
            _spawner.Reset(capacity);
            return new SandFlowSimulation(_tank, _interior, _container, _spawner, new GrainTransferQueue(256), rule, Yellow, 1f, 0, 8);
        }

        private void RunUntilComplete(SandFlowSimulation flow, OutletSpan span, int maxTicks) {
            for (int i = 0; i < maxTicks && !_container.IsComplete; i++) {
                flow.Tick(true, span, true);
            }
        }

        private void FillTank(int colorId, int rows) {
            _tank.Clear();
            for (int y = 0; y < rows; y++) {
                for (int x = 0; x < _tank.Width; x++) {
                    _tank.SetCell(x, y, SandCell.CreateGrain(colorId, 7));
                }
            }
        }

        private void RecordCompleted() {
            _completedEvents++;
        }
    }
}