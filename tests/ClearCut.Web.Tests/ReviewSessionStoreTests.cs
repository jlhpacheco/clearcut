using Xunit;
using ClearCut.Web.Models;
using ClearCut.Web.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        _store.Session.ResearchTraces["test"] = new ResearchTrace { FindingId = "test" };

        // Act
        _store.Reset();

        // Assert
        Assert.Empty(_store.Session.Findings);
        Assert.Empty(_store.Session.Dispositions);
        Assert.Empty(_store.Session.ReviewerNotes);
        Assert.Empty(_store.Session.ResearchTraces);
        Assert.False(_store.Session.IsAnalysisComplete);
    }

    [Fact]
    public async Task BeginResearchAsync_GuardsUnknownIds()
    {
        // Arrange
        await _store.BeginAnalysisAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _store.BeginResearchAsync("unknown-id"));
    }

    [Fact]
    public async Task BeginResearchAsync_PassesCurrentFindingToAgentClient()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "UseFixtures", "false" }
            })
            .Build();

        ReviewFinding? passedFinding = null;
        var mockHandler = new MockHttpMessageHandler(async req =>
        {
            var body = await req.Content!.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            passedFinding = new ReviewFinding
            {
                FindingId = doc.RootElement.GetProperty("finding_id").GetString() ?? "",
                Label = doc.RootElement.GetProperty("label").GetString() ?? "",
                Observation = doc.RootElement.GetProperty("observation").GetString() ?? "",
                ResearchObjective = doc.RootElement.GetProperty("research_objective").GetString() ?? ""
            };

            var ndjson = "{\"status\":\"ready\",\"objective\":\"Verify\",\"session_id\":\"s-1\",\"search_id\":\"sh-1\",\"retrieval_time\":\"now\",\"queries\":[\"query1\"],\"evidence\":[{\"title\":\"E\"}]}\n";
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(ndjson, System.Text.Encoding.UTF8, "application/x-ndjson")
            };
        });

        var client = new AgentClient(new HttpClient(mockHandler), config);
        var store = new ReviewSessionStore(client);

        var finding = new ReviewFinding
        {
            FindingId = "find-01-brand",
            Label = "LumaLeaf Logo",
            Observation = "Green stylized leaf logo",
            ResearchObjective = "Determine if matches registered trademarks"
        };
        store.Session.Findings.Add(finding);
        store.Session.IsAnalysisComplete = true;

        // Act
        await store.BeginResearchAsync("find-01-brand");

        // Assert
        Assert.NotNull(passedFinding);
        Assert.Equal("find-01-brand", passedFinding.FindingId);
        Assert.Equal("LumaLeaf Logo", passedFinding.Label);
        Assert.Equal("Green stylized leaf logo", passedFinding.Observation);
        Assert.Equal("Determine if matches registered trademarks", passedFinding.ResearchObjective);
    }

    [Fact]
    public async Task BeginResearchAsync_PreservesTraceAndResetClearsIt()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "UseFixtures", "false" } })
            .Build();

        var mockHandler = new MockHttpMessageHandler(req =>
        {
            var ndjson = "{\"status\":\"ready\",\"objective\":\"Verify Objective\",\"session_id\":\"sess-999\",\"search_id\":\"search-888\",\"retrieval_time\":\"2026-09-02T12:00:00Z\",\"queries\":[\"query1\"],\"evidence\":[{\"title\":\"E\"}]}\n";
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(ndjson, System.Text.Encoding.UTF8, "application/x-ndjson")
            });
        });

        var client = new AgentClient(new HttpClient(mockHandler), config);
        var store = new ReviewSessionStore(client);
        var finding = new ReviewFinding
        {
            FindingId = "find-01-brand",
            Label = "LumaLeaf Logo",
            Observation = "Green stylized leaf logo",
            ResearchObjective = "Verify Objective"
        };
        store.Session.Findings.Add(finding);
        store.Session.IsAnalysisComplete = true;

        await store.BeginResearchAsync("find-01-brand");

        Assert.True(store.Session.ResearchTraces.TryGetValue("find-01-brand", out var trace));
        Assert.Equal("Verify Objective", trace.Objective);
        Assert.Equal("sess-999", trace.SessionId);
        Assert.Equal("search-888", trace.SearchId);
        Assert.Equal("2026-09-02T12:00:00Z", trace.RetrievalTime);
        Assert.Single(trace.Queries);
        Assert.Equal("query1", trace.Queries[0]);

        store.Reset();
        Assert.Empty(store.Session.ResearchTraces);
    }

    [Fact]
    public async Task BeginResearchAsync_RequiresOneToThreeNonblankQueries()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "UseFixtures", "false" } })
            .Build();

        var mockHandler = new MockHttpMessageHandler(req =>
        {
            var ndjson = "{\"status\":\"ready\",\"objective\":\"Verify Objective\",\"session_id\":\"sess-999\",\"search_id\":\"search-888\",\"retrieval_time\":\"2026-09-02T12:00:00Z\",\"queries\":[\" \", \"\"],\"evidence\":[{\"title\":\"E\"}]}\n";
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(ndjson, System.Text.Encoding.UTF8, "application/x-ndjson")
            });
        });

        var client = new AgentClient(new HttpClient(mockHandler), config);
        var store = new ReviewSessionStore(client);
        var finding = new ReviewFinding
        {
            FindingId = "find-01-brand",
            Label = "LumaLeaf Logo",
            Observation = "Green stylized leaf logo",
            ResearchObjective = "Verify Objective"
        };
        store.Session.Findings.Add(finding);
        store.Session.IsAnalysisComplete = true;

        await store.BeginResearchAsync("find-01-brand");

        Assert.Equal("incomplete", store.Session.ResearchStatus["find-01-brand"]);
    }

    [Fact]
    public async Task FixtureSummaries_MakeNoExecutedSearchOutcomeClaim()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "UseFixtures", "true" }
            })
            .Build();

        var client = new AgentClient(new HttpClient(), config);

        // Act & Assert for brand
        var brandFinding = new ReviewFinding
        {
            FindingId = "find-01-brand",
            Label = "LumaLeaf Logo",
            Observation = "Green stylized leaf logo",
            ResearchObjective = "Determine if matches registered trademarks"
        };
        var brandEvents = new List<ResearchEvent>();
        await foreach (var ev in client.ResearchStreamAsync(brandFinding))
        {
            brandEvents.Add(ev);
        }
        var brandReady = brandEvents[^1];
        Assert.NotNull(brandReady.Evidence);
        foreach (var ev in brandReady.Evidence)
        {
            Assert.Contains("Fixture demonstration—no search executed.", ev.RelevanceSummary);
            Assert.Contains("starting point", ev.RelevanceSummary);
            Assert.Contains("human search", ev.RelevanceSummary);
            Assert.Contains("No final clearance or ownership conclusion", ev.RelevanceSummary);
        }
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        public MockHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return _handler(request);
        }
    }
}
