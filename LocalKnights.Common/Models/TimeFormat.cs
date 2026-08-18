namespace LocalKnights.Common.Models
{
    /// <summary>
    /// Enumeration of time formats for chess games.
    /// </summary>
    public enum TimeFormat
    {
        /// <summary>Bullet: 1 minute to 2 minutes per side.</summary>
        Bullet,
        /// <summary>Blitz: 3 minutes to 8 minutes per side.</summary>
        Blitz,
        /// <summary>Rapid: 10 minutes to 60 minutes per side.</summary>
        Rapid,
        /// <summary>Classical: More than 60 minutes per side.</summary>
        Classical
    }
}