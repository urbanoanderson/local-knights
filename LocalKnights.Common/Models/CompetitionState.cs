namespace LocalKnights.Common.Models
{
    /// <summary>
    /// Enumeration of competition states.
    /// </summary>
    public enum CompetitionState
    {
        /// <summary>Competition is about to start or in the process of being set up.</summary>
        Starting,
        /// <summary>Competition is currently ongoing.</summary>
        Ongoing,
        /// <summary>Competition has been completed.</summary>
        Completed,
    }
}