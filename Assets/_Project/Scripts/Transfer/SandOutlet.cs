using System;

using SandGameplay.Sand;

namespace SandGameplay.Transfer {
    /// <summary>
    /// Moves accepted grains from the tank's bottom row (inside the outlet span) into the transfer queue.
    /// Grains the rule rejects stay where they are and block their column, as in the reference game.
    /// </summary>
    public static class SandOutlet {
        public static int Transfer(SandGrid tank, OutletSpan span, ContainerColorRule rule, int containerColorId, int maxGrains, bool leftToRight, GrainTransferQueue queue) {
            if (tank == null) {
                throw new ArgumentNullException(nameof(tank));
            }

            if (queue == null) {
                throw new ArgumentNullException(nameof(queue));
            }

            if (span.IsEmpty) {
                return 0;
            }

            int transferred = 0;
            int columns = span.Width;
            for (int i = 0; i < columns; i++) {
                int x = leftToRight ? span.TankStartX + i : span.TankEndX - 1 - i;
                ushort cell = tank.GetCell(x, 0);
                if (!SandCell.IsGrain(cell) || !rule.Accepts(containerColorId, SandCell.GetColorId(cell))) {
                    continue;
                }

                if (!queue.TryEnqueue(cell, x - span.InteriorOffsetX)) {
                    break;
                }

                tank.Remove(x, 0);
                transferred++;
                if (maxGrains > 0 && transferred >= maxGrains) {
                    break;
                }
            }

            return transferred;
        }
    }
}