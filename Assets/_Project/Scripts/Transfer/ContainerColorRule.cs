namespace SandGameplay.Transfer {
    public enum ContainerColorRule {
        /// <summary>The box only accepts grains of its own color; other grains over the opening block the flow.</summary>
        MatchColor = 0,

        /// <summary>The box accepts every grain.</summary>
        AcceptAny = 1
    }
}