using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using LocalKnights.Common.Models;

namespace LocalKnights.Common.Services.Database
{
    /// <summary>
    /// Entity Framework DbContext for the application.
    /// Manages database operations for clubs, players, games, and competitions.
    /// </summary>
    public class ChessClubContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChessClubContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by the context.</param>
        public ChessClubContext(DbContextOptions<ChessClubContext> options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the DbSet for club preferences (single row containing club settings).
        /// </summary>
        public DbSet<ClubPreferences> ClubPreferences { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for players.
        /// </summary>
        public DbSet<Player> Players { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for games.
        /// </summary>
        public DbSet<Game> Games { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for competitions.
        /// </summary>
        public DbSet<Competition> Competitions { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for competition pairings (replaces CompetitionRound).
        /// </summary>
        public DbSet<CompetitionPairing> CompetitionPairings { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for competition participants (join table between Competition and Player).
        /// </summary>
        public DbSet<CompetitionParticipant> CompetitionParticipants { get; set; }

        /// <summary>
        /// Configures the model for the database context.
        /// Sets up entity relationships, indexes, and value converters.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ClubPreferences - single row table with Id always = 1
            ValueConverter<List<string>?, string?> socialMediaLinksConverter = new ValueConverter<List<string>?, string?>(
                links => SerializeSocialMediaLinks(links),
                json => DeserializeSocialMediaLinks(json));

            modelBuilder.Entity<ClubPreferences>()
                .Property(c => c.SocialMediaLinks)
                .HasConversion(socialMediaLinksConverter);

            // Player
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasIndex(p => p.IdentifierName)
                    .IsUnique();

                entity.OwnsOne(p => p.BulletRating);
                entity.OwnsOne(p => p.BlitzRating);
                entity.OwnsOne(p => p.RapidRating);
                entity.OwnsOne(p => p.ClassicalRating);
            });

            // Game
            modelBuilder.Entity<Game>(entity =>
            {
                entity.OwnsOne(g => g.WhitePlayerRating);
                entity.OwnsOne(g => g.BlackPlayerRating);
            });

            // Competition
            modelBuilder.Entity<Competition>(entity =>
            {
                entity.HasMany(c => c.CompetitionParticipants)
                    .WithOne(cp => cp.Competition)
                    .HasForeignKey(cp => cp.CompetitionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CompetitionParticipant>(entity =>
            {
                entity.HasKey(cp => new { cp.CompetitionId, cp.ParticipantIdentifierName });

                entity.OwnsOne(cp => cp.OldRating);
                entity.OwnsOne(cp => cp.NewRating);
            });

            modelBuilder.Entity<CompetitionPairing>(entity =>
            {
                entity.HasKey(cp => new { cp.CompetitionId, cp.RoundNumber, cp.TableNumber });

                entity.HasOne(cp => cp.Competition)
                    .WithMany(c => c.CompetitionPairings)
                    .HasForeignKey(cp => cp.CompetitionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Serializes a list of social media links to JSON format for database storage.
        /// </summary>
        /// <param name="links">The list of social media links to serialize.</param>
        /// <returns>JSON string representation of the links, or null if the input is null.</returns>
        private static string? SerializeSocialMediaLinks(List<string>? links)
        {
            return links == null ? null : JsonSerializer.Serialize(links);
        }

        /// <summary>
        /// Deserializes a JSON string to a list of social media links.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>List of social media links, or null if the JSON string is null or empty.</returns>
        private static List<string>? DeserializeSocialMediaLinks(string? json)
        {
            return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<List<string>>(json);
        }
    }
}