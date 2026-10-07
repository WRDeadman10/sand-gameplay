using System.Collections.Generic;

using NUnit.Framework;

using SandGameplay.Data;
using SandGameplay.Sand;

using UnityEngine;

namespace SandGameplay.Tests {
    /// <summary>
    /// Regression and validation tests for all level configurations.
    /// Ensures that no level contains phantom containers (e.g. from stray pixels),
    /// no sand lacks an accepting container, and no container starts on top of wall blockers or other containers.
    /// </summary>
    public sealed class LevelIntegrityTests {
        private const int MinimumGrainsPerContainer = 50;

        private LevelConfig[] LoadAllLevels() {
            LevelConfig[] levels = Resources.LoadAll<LevelConfig>("Levels");
            Assert.IsNotNull(levels, "Resources/Levels should contain level configurations.");
            Assert.Greater(levels.Length, 0, "At least one level configuration must exist.");
            return levels;
        }

        [Test]
        public void AllLevels_EachContainerHasMatchingSandInTank() {
            LevelConfig[] levels = LoadAllLevels();

            for (int i = 0; i < levels.Length; i++) {
                LevelConfig cfg = levels[i];
                SandGrid grid = new SandGrid(cfg.TankSize.x * 24, cfg.TankSize.y * 24, 1337u);
                cfg.TankPattern.Fill(grid, cfg.Palette, 1337u);

                Dictionary<int, int> sandCounts = CountGrainsPerColor(grid);

                for (int c = 0; c < cfg.Containers.Count; c++) {
                    ContainerDefinition def = cfg.Containers[c];
                    int grainCount = sandCounts.GetValueOrDefault(def.colorId, 0);

                    Assert.GreaterOrEqual(
                        grainCount,
                        MinimumGrainsPerContainer,
                        $"[{cfg.name}] Container {c} (colorId {def.colorId}) has only {grainCount} matching grains in tank. Phantom containers from stray pixels are prohibited.");
                }
            }
        }

        [Test]
        public void AllLevels_EachSandColorHasAcceptingContainer() {
            LevelConfig[] levels = LoadAllLevels();

            for (int i = 0; i < levels.Length; i++) {
                LevelConfig cfg = levels[i];
                SandGrid grid = new SandGrid(cfg.TankSize.x * 24, cfg.TankSize.y * 24, 1337u);
                cfg.TankPattern.Fill(grid, cfg.Palette, 1337u);

                Dictionary<int, int> sandCounts = CountGrainsPerColor(grid);
                HashSet<int> containerColors = new HashSet<int>();
                for (int c = 0; c < cfg.Containers.Count; c++) {
                    containerColors.Add(cfg.Containers[c].colorId);
                }

                foreach (KeyValuePair<int, int> kvp in sandCounts) {
                    if (kvp.Value >= MinimumGrainsPerContainer) {
                        Assert.IsTrue(
                            containerColors.Contains(kvp.Key),
                            $"[{cfg.name}] Tank contains {kvp.Value} grains of colorId {kvp.Key}, but no container accepts this color.");
                    }
                }
            }
        }

        [Test]
        public void AllLevels_NoContainerOverlapsWallBlocker() {
            LevelConfig[] levels = LoadAllLevels();

            for (int i = 0; i < levels.Length; i++) {
                LevelConfig cfg = levels[i];

                for (int c = 0; c < cfg.Containers.Count; c++) {
                    ContainerDefinition def = cfg.Containers[c];

                    for (int w = 0; w < cfg.WallCells.Count; w++) {
                        Vector2Int wall = cfg.WallCells[w];
                        bool overlaps = wall.x >= def.startCell.x &&
                                        wall.x < def.startCell.x + def.footprint.x &&
                                        wall.y >= def.startCell.y &&
                                        wall.y < def.startCell.y + def.footprint.y;

                        Assert.IsFalse(
                            overlaps,
                            $"[{cfg.name}] Container {c} (colorId {def.colorId}) at ({def.startCell.x},{def.startCell.y}) overlaps wall blocker at ({wall.x},{wall.y}). Boxes must never start on blockers.");
                    }
                }
            }
        }

        [Test]
        public void AllLevels_NoContainerOverlapsAnotherContainer() {
            LevelConfig[] levels = LoadAllLevels();

            for (int i = 0; i < levels.Length; i++) {
                LevelConfig cfg = levels[i];

                for (int c1 = 0; c1 < cfg.Containers.Count; c1++) {
                    ContainerDefinition def1 = cfg.Containers[c1];
                    Rect r1 = new Rect(def1.startCell.x, def1.startCell.y, def1.footprint.x, def1.footprint.y);

                    for (int c2 = c1 + 1; c2 < cfg.Containers.Count; c2++) {
                        ContainerDefinition def2 = cfg.Containers[c2];
                        Rect r2 = new Rect(def2.startCell.x, def2.startCell.y, def2.footprint.x, def2.footprint.y);

                        Assert.IsFalse(
                            r1.Overlaps(r2),
                            $"[{cfg.name}] Container {c1} overlaps Container {c2}. Containers must start in distinct cells.");
                    }
                }
            }
        }

        [Test]
        public void AllLevels_AllContainersWithinBoardBounds() {
            LevelConfig[] levels = LoadAllLevels();

            for (int i = 0; i < levels.Length; i++) {
                LevelConfig cfg = levels[i];

                for (int c = 0; c < cfg.Containers.Count; c++) {
                    ContainerDefinition def = cfg.Containers[c];

                    Assert.GreaterOrEqual(def.startCell.x, 0, $"[{cfg.name}] Container {c} x < 0");
                    Assert.GreaterOrEqual(def.startCell.y, 0, $"[{cfg.name}] Container {c} y < 0");
                    Assert.LessOrEqual(def.startCell.x + def.footprint.x, cfg.BoardSize.x, $"[{cfg.name}] Container {c} exceeds board width");
                    Assert.LessOrEqual(def.startCell.y + def.footprint.y, cfg.BoardSize.y, $"[{cfg.name}] Container {c} exceeds board height");
                }
            }
        }

        private static Dictionary<int, int> CountGrainsPerColor(SandGrid grid) {
            Dictionary<int, int> sandCounts = new Dictionary<int, int>();
            for (int y = 0; y < grid.Height; y++) {
                for (int x = 0; x < grid.Width; x++) {
                    ushort cell = grid.GetCell(x, y);
                    if (SandCell.IsGrain(cell)) {
                        int cid = SandCell.GetColorId(cell);
                        sandCounts[cid] = sandCounts.GetValueOrDefault(cid, 0) + 1;
                    }
                }
            }
            return sandCounts;
        }
    }
}
