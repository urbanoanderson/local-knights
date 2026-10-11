using System.Reflection;
using System.Text.Json;
using LocalKnights.Common.Models;
using LocalKnights.Common.Services;

namespace LocalKnights.Common.Tests.Services;

/// <summary>
/// Tests defensive parsing behavior for FIDE API rating data.
/// </summary>
public sealed class FideApiServiceTests
{
    /// <summary>
    /// Verifies null rating groups are ignored while string and numeric ratings are parsed.
    /// </summary>
    [Fact]
    public void TryAddRating_WithNullAndMixedValues_IgnoresInvalidValues()
    {
        using JsonDocument document = JsonDocument.Parse(
            """
            {
              "bullet": { "rating": "1800" },
              "blitz": { "rating": 1938 },
              "rapid": null,
              "standard": { "rating": null }
            }
            """);
        Dictionary<TimeFormat, double> ratings = [];

        InvokeTryAddRating(document.RootElement, "bullet", TimeFormat.Bullet, ratings);
        InvokeTryAddRating(document.RootElement, "blitz", TimeFormat.Blitz, ratings);
        InvokeTryAddRating(document.RootElement, "rapid", TimeFormat.Rapid, ratings);
        InvokeTryAddRating(document.RootElement, "standard", TimeFormat.Classical, ratings);

        Assert.Equal(1800d, ratings[TimeFormat.Bullet]);
        Assert.Equal(1938d, ratings[TimeFormat.Blitz]);
        Assert.False(ratings.ContainsKey(TimeFormat.Rapid));
        Assert.False(ratings.ContainsKey(TimeFormat.Classical));
    }

    private static void InvokeTryAddRating(
        JsonElement currentElement,
        string propertyName,
        TimeFormat timeFormat,
        IDictionary<TimeFormat, double> ratings)
    {
        MethodInfo method = typeof(FideApiService).GetMethod(
            "TryAddRating",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, [currentElement, propertyName, timeFormat, ratings]);
    }
}