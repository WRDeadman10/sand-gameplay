using System;

using SandGameplay.Sand;

namespace SandGameplay.Transfer {
    public static class ContainerColorRuleExtensions {
        public static bool Accepts(this ContainerColorRule rule, int containerColorId, int grainColorId) {
            return rule == ContainerColorRule.AcceptAny || containerColorId == grainColorId;
        }

        /// <summary>Number of grains in the grid this rule would accept; used as the box capacity.</summary>
        public static int CountAcceptable(this ContainerColorRule rule, SandGrid grid, int containerColorId) {
            if (grid == null) {
                throw new ArgumentNullException(nameof(grid));
            }

            return rule == ContainerColorRule.AcceptAny ? grid.GrainCount : grid.CountColor(containerColorId);
        }
    }
}