using UnityEngine;

namespace SandGameplay.Transfer {
    /// <summary>Decides whether a box is docked under the tank and which tank columns drain into it.</summary>
    public static class DockResolver {
        /// <param name="openingMinX">World x of the left edge of the box opening.</param>
        /// <param name="boxTopY">World y of the box's top edge.</param>
        /// <param name="interiorWidth">Box opening width in grains.</param>
        /// <param name="tankInteriorWorldRect">Tank interior in world space.</param>
        /// <param name="grainsPerCell">Grains per world unit.</param>
        /// <param name="tolerance">Allowed vertical gap (world units) between the box top and the tank bottom.</param>
        /// <param name="tankWidth">Tank width in grains.</param>
        /// <param name="span">The draining span when docked.</param>
        public static bool TryResolve(float openingMinX, float boxTopY, int interiorWidth, Rect tankInteriorWorldRect, int grainsPerCell, float tolerance, int tankWidth, out OutletSpan span) {
            span = default(OutletSpan);
            if (interiorWidth <= 0 || tankWidth <= 0 || grainsPerCell <= 0) {
                return false;
            }

            if (Mathf.Abs(tankInteriorWorldRect.yMin - boxTopY) > tolerance) {
                return false;
            }

            int offset = Mathf.RoundToInt((openingMinX - tankInteriorWorldRect.xMin) * grainsPerCell);
            int start = Mathf.Max(0, offset);
            int end = Mathf.Min(tankWidth, offset + interiorWidth);
            if (end <= start) {
                return false;
            }

            span = new OutletSpan(start, end, offset);
            return true;
        }
    }
}