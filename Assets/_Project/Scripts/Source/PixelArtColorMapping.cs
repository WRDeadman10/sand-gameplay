namespace SandGameplay.Source {
    public enum PixelArtColorMapping {
        /// <summary>Pixel luminance selects a tone of the pattern's single color (always completable).</summary>
        ToneOfFillColor = 0,

        /// <summary>Pixel color selects the nearest palette color at a middle tone (multi-color tanks).</summary>
        NearestPaletteColor = 1
    }
}