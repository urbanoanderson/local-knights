namespace LocalKnights.Common.Models
{
    /// <summary>
    /// Specifies how initial seeding of participant pairings in a competition is determined.
    /// </summary>
    public enum CompetitionInitialSeeding
    {
        /// <summary>
        /// Seed participant pairings by rating (highest first).
        /// </summary>
        Rating = 0,

        /// <summary>
        /// Seed participant pairings randomly.
        /// </summary>
        Random = 1
    }
}
