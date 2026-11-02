using LocalKnights.Common.Helpers.ChessAlgorithms.Standings;
using LocalKnights.Common.Models;
using LocalKnights.WindowsDesktop.DisplayObjects;
using LocalKnights.WindowsDesktop.Helpers;

namespace LocalKnights.WindowsDesktop.Tests;

public sealed class FormatHelperTests
{
    [Fact]
    public void FormatCompetitionPageStandingTableItem_SortsClassificationNumerically()
    {
        CompetitionPageStandingTableItem[] standings =
        [
            CreateStandingTableItem(10),
            CreateStandingTableItem(2),
        ];

        Assert.Equal(
            new[] { "#2", "#10" },
            standings.OrderBy(standing => standing.ClassificationSortKey).Select(standing => standing.Classification));
    }

    private static CompetitionPageStandingTableItem CreateStandingTableItem(int classification)
    {
        CompetitionStandingEntry standingEntry = new()
        {
            Classification = classification,
            Participant = new CompetitionParticipant
            {
                ParticipantIdentifierName = $"player{classification}",
                OldRating = new ChessRating(),
            },
        };

        return FormatHelper.FormatCompetitionPageStandingTableItem(new ClubPreferences(), standingEntry, null!);
    }
}
