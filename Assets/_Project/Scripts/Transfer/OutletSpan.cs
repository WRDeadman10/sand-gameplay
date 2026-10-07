namespace SandGameplay.Transfer {
    /// <summary>
    /// The part of the tank's bottom row that drains into a docked box, in tank grain columns
    /// [<see cref="TankStartX"/>, <see cref="TankEndX"/>). Box interior column = tank column - <see cref="InteriorOffsetX"/>.
    /// </summary>
    public readonly struct OutletSpan {
        public OutletSpan(int tankStartX, int tankEndX, int interiorOffsetX) {
            TankStartX = tankStartX;
            TankEndX = tankEndX;
            InteriorOffsetX = interiorOffsetX;
        }

        public int TankStartX { get; }

        public int TankEndX { get; }

        public int InteriorOffsetX { get; }

        public int Width {
            get {
                return TankEndX - TankStartX;
            }
        }

        public bool IsEmpty {
            get {
                return TankEndX <= TankStartX;
            }
        }
    }
}