namespace LocalKnights.Common.Models
{
    /// <summary>
    /// Defines the supported rating systems for the application.
    /// </summary>
    public enum RatingSystem
    {
        /// <summary>Glicko-2 rating system (Default).</summary>
        Glicko2,
        /// <summary>ELO rating system.</summary>
        ELO,
    }
}