using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using LocalKnights.Common.Helpers;
using LocalKnights.Common.Models;
using LocalKnights.WindowsDesktop.DisplayObjects;
using LocalKnights.WindowsDesktop.Views;

namespace LocalKnights.WindowsDesktop.Tests;

public sealed class CompetitionPagePairingTableItemTests
{
    [Fact]
    public void SelectionChanged_WhenSelectionIsOnlyRemoved_DoesNotCommitResult()
    {
        CompetitionPagePairingTableItem item = CreatePairingTableItem(GameResult.WhiteWins);
        bool resultCommitted = false;
        item.OnSetResult += (_, _) => resultCommitted = true;
        SelectionChangedEventArgs eventArgs = new(
            Selector.SelectionChangedEvent,
            new object[] { CommonConstants.WhiteWinResultString },
            Array.Empty<object>());

        if (CompetitionView.TryGetAddedResult(eventArgs, out string selectedResult))
        {
            item.CommitSelectedResult(selectedResult);
        }

        Assert.False(resultCommitted);
    }

    [Fact]
    public void CommitSelectedResult_WhenBlankIsExplicitlySelected_CommitsNullResult()
    {
        CompetitionPagePairingTableItem item = CreatePairingTableItem(GameResult.WhiteWins);
        GameResult? committedResult = GameResult.WhiteWins;
        item.OnSetResult += (_, result) => committedResult = result;
        SelectionChangedEventArgs eventArgs = new(
            Selector.SelectionChangedEvent,
            new object[] { CommonConstants.WhiteWinResultString },
            new object[] { string.Empty });

        bool hasAddedResult = CompetitionView.TryGetAddedResult(eventArgs, out string selectedResult);
        item.CommitSelectedResult(selectedResult);

        Assert.True(hasAddedResult);
        Assert.Null(committedResult);
    }

    [Fact]
    public void CommitSelectedResult_WhenResultIsExplicitlySelected_CommitsParsedResult()
    {
        CompetitionPagePairingTableItem item = CreatePairingTableItem(result: null);
        GameResult? committedResult = null;
        item.OnSetResult += (_, result) => committedResult = result;
        SelectionChangedEventArgs eventArgs = new(
            Selector.SelectionChangedEvent,
            new object[] { string.Empty },
            new object[] { CommonConstants.BlackWinResultString });

        bool hasAddedResult = CompetitionView.TryGetAddedResult(eventArgs, out string selectedResult);
        item.CommitSelectedResult(selectedResult);

        Assert.True(hasAddedResult);
        Assert.Equal(GameResult.BlackWins, committedResult);
    }

    private static CompetitionPagePairingTableItem CreatePairingTableItem(GameResult? result)
    {
        Competition competition = new()
        {
            TimeFormat = TimeFormat.Blitz,
        };
        competition.CompetitionParticipants.Add(new CompetitionParticipant
        {
            ParticipantIdentifierName = "white",
            OldRating = new ChessRating(),
        });
        competition.CompetitionParticipants.Add(new CompetitionParticipant
        {
            ParticipantIdentifierName = "black",
            OldRating = new ChessRating(),
        });
        CompetitionPairing pairing = new()
        {
            Competition = competition,
            WhitePlayerIdentifierName = "white",
            BlackPlayerIdentifierName = "black",
            Result = result,
        };

        return new CompetitionPagePairingTableItem(
            pairing,
            new Player { IdentifierName = "white" },
            new Player { IdentifierName = "black" },
            new ClubPreferences(),
            canNavigateToGame: false);
    }
}
