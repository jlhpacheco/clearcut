using Xunit;
using ClearCut.Web.Models;
using ClearCut.Web.Services;
using Microsoft.Extensions.Configuration;

namespace ClearCut.Web.Tests;

public class ReviewSessionStoreTests
{
    private readonly ReviewSessionStore _store;

    public ReviewSessionStoreTests()
    {
        // Setup minimal mock configurations for testing
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "UseFixtures", "true" }
            })
            .Build();

        var client = new AgentClient(new HttpClient(), config);
        _store = new ReviewSessionStore(client);
    }

    [Fact]
    public async Task BeginAnalysisAsync_SucceedsAndFillsFindingsChronologically()
    {
        // Act
        await _store.BeginAnalysisAsync();

        // Assert
        Assert.False(_store.IsOperationActive);
        Assert.True(_store.Session.IsAnalysisComplete);
        Assert.Equal(3, _store.Session.Findings.Count);

        // Assert chronological order: 4.5s < 15.0s < 25.0s
        Assert.True(_store.Session.Findings[0].StartSeconds < _store.Session.Findings[1].StartSeconds);
        Assert.True(_store.Session.Findings[1].StartSeconds < _store.Session.Findings[2].StartSeconds);
    }

    [Fact]
    public async Task CanDismiss_GuardedUntilEvidenceIsReady()
    {
        // Arrange
        await _store.BeginAnalysisAsync();
        var brandFindingId = "find-01-brand";

        // Act & Assert: Initially cannot dismiss as status is pending/none
        Assert.False(_store.CanDismiss(brandFindingId));
        Assert.Throws<InvalidOperationException>(() => _store.SetDisposition(brandFindingId, Disposition.Dismiss));

        // Act: Perform research to retrieve evidence
        await _store.BeginResearchAsync(brandFindingId);

        // Assert: Evidence is now ready, allowing Dismiss
        Assert.True(_store.CanDismiss(brandFindingId));
        _store.SetDisposition(brandFindingId, Disposition.Dismiss);
        Assert.Equal(Disposition.Dismiss, _store.Session.Dispositions[brandFindingId]);
    }

    [Fact]
    public async Task CanPrint_OnlyTrueWhenAllThreeFindingsHaveDispositions()
    {
        // Arrange
        await _store.BeginAnalysisAsync();
        var f1 = _store.Session.Findings[0].FindingId;
        var f2 = _store.Session.Findings[1].FindingId;
        var f3 = _store.Session.Findings[2].FindingId;

        // Initially CanPrint is false
        Assert.False(_store.CanPrint());

        // Select dispositions for 2 of 3 findings
        _store.SetDisposition(f2, Disposition.Investigate);
        _store.SetDisposition(f3, Disposition.Replace);
        Assert.False(_store.CanPrint());

        // Gather evidence to allow dismiss of the 1st finding
        await _store.BeginResearchAsync(f1);
        _store.SetDisposition(f1, Disposition.Dismiss);

        // All 3 assigned human dispositions -> Unlocks print readiness
        Assert.True(_store.CanPrint());
    }

    [Fact]
    public void Reset_ClearsAllSessionData()
    {
        // Arrange
        _store.Session.Findings.Add(new ReviewFinding { FindingId = "test" });
        _store.Session.Dispositions["test"] = Disposition.Investigate;
        _store.Session.ReviewerNotes["test"] = "some note";

        // Act
        _store.Reset();

        // Assert
        Assert.Empty(_store.Session.Findings);
        Assert.Empty(_store.Session.Dispositions);
        Assert.Empty(_store.Session.ReviewerNotes);
        Assert.False(_store.Session.IsAnalysisComplete);
    }
}
