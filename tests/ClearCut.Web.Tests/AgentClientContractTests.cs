using System.Text.Json;
using Xunit;
using ClearCut.Web.Models;
using ClearCut.Web.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClearCut.Web.Tests;

public class AgentClientContractTests
{
    private string GetContractsPath()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            var path = Path.Combine(dir, "contracts", "golden-review.json");
            if (File.Exists(path)) return path;
            dir = Path.GetDirectoryName(dir);
        }
        throw new FileNotFoundException("Could not locate contracts/golden-review.json in any parent folder.");
    }

    [Fact]
    public void Deserializes_GoldenReviewContract_Successfully()
    {
        // Arrange
        var contractPath = GetContractsPath();
        var jsonText = File.ReadAllText(contractPath);

        // Act & Deserialize
        var doc = JsonDocument.Parse(jsonText);
        var findingsJson = doc.RootElement.GetProperty("findings").GetRawText();
        var evidenceJson = doc.RootElement.GetProperty("evidence_sources").GetRawText();

        var findings = JsonSerializer.Deserialize<List<ReviewFinding>>(findingsJson);
        var evidence = JsonSerializer.Deserialize<List<EvidenceSource>>(evidenceJson);

        // Assert Findings
        Assert.NotNull(findings);
        Assert.Equal(3, findings.Count);

        var f1 = findings[0];
        Assert.Equal("find-01-brand", f1.FindingId);
        Assert.Equal("brand_mark", f1.Category);
        Assert.Equal(4.5, f1.StartSeconds);
        Assert.Equal(12.0, f1.EndSeconds);
        Assert.Equal("LumaLeaf Logo", f1.Label);
        Assert.Contains("LumaLeaf Energy", f1.Observation);
        Assert.Equal("attention", f1.ReviewPriority);
        Assert.Equal("Determine if 'LumaLeaf Energy' or the green leaf logo matches registered trademarks or existing brand marks.", f1.ResearchObjective);

        // Assert Evidence
        Assert.NotNull(evidence);
        Assert.Single(evidence);
        
        var ev = evidence[0];
        Assert.Equal("LumaLeaf Fictional Energy Study", ev.Title);
        Assert.Equal("clearcut.web", ev.Publisher);
        Assert.Equal("2026-09-02", ev.RetrievalDate);
        Assert.Contains("CC-EVID-9F4D", ev.RelevanceSummary);
        Assert.Equal("http://localhost:5000/evidence/lumaleaf-energy-study", ev.Url);
    }

    [Fact]
    public async Task ResearchStreamAsync_LiveMode_SendsCorrectJsonAndParsesMetadata()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "UseFixtures", "false" },
                { "CLEARCUT_AGENT_BASE_URL", "http://localhost:8000" }
            })
            .Build();

        HttpRequestMessage? capturedRequest = null;
        string? capturedBody = null;

        var mockHandler = new MockHttpMessageHandler(async req =>
        {
            capturedRequest = req;
            capturedBody = await req.Content!.ReadAsStringAsync();

            var ndjson = "{\"status\":\"preparing\",\"task\":\"Formulating...\"}\n" +
                         "{\"status\":\"ready\",\"queries\":[\"q1\",\"q2\"],\"objective\":\"Verify clearance\",\"session_id\":\"sess-123\",\"search_id\":\"search-456\",\"retrieval_time\":\"2026-09-02T12:00:00Z\",\"evidence\":[{\"title\":\"Test Source\",\"publisher\":\"test\",\"retrieval_date\":\"2026-09-02\",\"relevance_summary\":\"summary\",\"url\":\"http://test\"}]}\n";

            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(ndjson, System.Text.Encoding.UTF8, "application/x-ndjson")
            };
        });

        var httpClient = new HttpClient(mockHandler);
        var client = new AgentClient(httpClient, config);

        var finding = new ReviewFinding
        {
            FindingId = "find-test",
            Label = "Test Label",
            Observation = "Test Observation",
            ResearchObjective = "Test Objective"
        };

        // Act
        var events = new List<ResearchEvent>();
        await foreach (var ev in client.ResearchStreamAsync(finding))
        {
            events.Add(ev);
        }

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal("http://localhost:8000/v1/research/stream", capturedRequest.RequestUri?.ToString());
        Assert.NotNull(capturedBody);

        using var doc = JsonDocument.Parse(capturedBody);
        var root = doc.RootElement;
        Assert.Equal("find-test", root.GetProperty("finding_id").GetString());
        Assert.Equal("Test Label", root.GetProperty("label").GetString());
        Assert.Equal("Test Observation", root.GetProperty("observation").GetString());
        Assert.Equal("Test Objective", root.GetProperty("research_objective").GetString());
        Assert.True(root.TryGetProperty("session_id", out var sessIdProp));
        Assert.False(string.IsNullOrEmpty(sessIdProp.GetString()));

        Assert.Equal(2, events.Count);
        Assert.Equal("preparing", events[0].Status);

        var readyEvent = events[1];
        Assert.Equal("ready", readyEvent.Status);
        Assert.NotNull(readyEvent.Queries);
        Assert.Contains("q1", readyEvent.Queries);
        Assert.Equal("Verify clearance", readyEvent.Objective);
        Assert.Equal("sess-123", readyEvent.SessionId);
        Assert.Equal("search-456", readyEvent.SearchId);
        Assert.Equal("2026-09-02T12:00:00Z", readyEvent.RetrievalTime);
        Assert.NotNull(readyEvent.Evidence);
        Assert.Single(readyEvent.Evidence);
        Assert.Equal("Test Source", readyEvent.Evidence[0].Title);
    }

    [Fact]
    public async Task ResearchStreamAsync_MalformedNdjson_FailsClosed()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "UseFixtures", "false" }
            })
            .Build();

        var mockHandler = new MockHttpMessageHandler(req =>
        {
            var ndjson = "{\"status\":\"preparing\"}\n{invalid-json}\n";
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(ndjson, System.Text.Encoding.UTF8, "application/x-ndjson")
            });
        });

        var httpClient = new HttpClient(mockHandler);
        var client = new AgentClient(httpClient, config);

        var finding = new ReviewFinding
        {
            FindingId = "find-test",
            Label = "Test Label",
            Observation = "Test Observation",
            ResearchObjective = "Test Objective"
        };

        // Act
        var events = new List<ResearchEvent>();
        await foreach (var ev in client.ResearchStreamAsync(finding))
        {
            events.Add(ev);
        }

        // Assert
        Assert.Equal(2, events.Count);
        Assert.Equal("preparing", events[0].Status);
        Assert.Equal("incomplete", events[1].Status);
        Assert.Equal("Malformed event received from stream.", events[1].Error);
    }

    [Fact]
    public async Task ResearchStreamAsync_PrematureEOF_YieldsIncomplete()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "UseFixtures", "false" } })
            .Build();

        var mockHandler = new MockHttpMessageHandler(req =>
        {
            var ndjson = "{\"status\":\"preparing\"}\n";
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(ndjson, System.Text.Encoding.UTF8, "application/x-ndjson")
            });
        });

        var client = new AgentClient(new HttpClient(mockHandler), config);
        var finding = new ReviewFinding { FindingId = "f", Label = "L", Observation = "O", ResearchObjective = "Obj" };

        var events = new List<ResearchEvent>();
        await foreach (var ev in client.ResearchStreamAsync(finding))
        {
            events.Add(ev);
        }

        Assert.Equal(2, events.Count);
        Assert.Equal("preparing", events[0].Status);
        Assert.Equal("incomplete", events[1].Status);
        Assert.Contains("prematurely", events[1].Error);
    }

    [Fact]
    public async Task ResearchStreamAsync_JsonNull_YieldsIncomplete()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "UseFixtures", "false" } })
            .Build();
        var mockHandler = new MockHttpMessageHandler(req => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("null\n", System.Text.Encoding.UTF8, "application/x-ndjson")
        }));
        var client = new AgentClient(new HttpClient(mockHandler), config);
        var finding = new ReviewFinding { FindingId = "f", Label = "L", Observation = "O", ResearchObjective = "Obj" };
        var events = new List<ResearchEvent>();
        await foreach (var ev in client.ResearchStreamAsync(finding)) events.Add(ev);
        Assert.Single(events);
        Assert.Equal("incomplete", events[0].Status);
    }

    [Fact]
    public async Task ResearchStreamAsync_UnknownStatus_YieldsIncomplete()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "UseFixtures", "false" } })
            .Build();
        var mockHandler = new MockHttpMessageHandler(req => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{\"status\":\"mystery\"}\n", System.Text.Encoding.UTF8, "application/x-ndjson")
        }));
        var client = new AgentClient(new HttpClient(mockHandler), config);
        var finding = new ReviewFinding { FindingId = "f", Label = "L", Observation = "O", ResearchObjective = "Obj" };
        var events = new List<ResearchEvent>();
        await foreach (var ev in client.ResearchStreamAsync(finding)) events.Add(ev);
        Assert.Single(events);
        Assert.Equal("incomplete", events[0].Status);
    }

    [Fact]
    public async Task ResearchStreamAsync_ReadyWithZeroEvidence_YieldsIncomplete()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "UseFixtures", "false" } })
            .Build();
        var mockHandler = new MockHttpMessageHandler(req => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{\"status\":\"ready\",\"evidence\":[]}\n", System.Text.Encoding.UTF8, "application/x-ndjson")
        }));
        var client = new AgentClient(new HttpClient(mockHandler), config);
        var finding = new ReviewFinding { FindingId = "f", Label = "L", Observation = "O", ResearchObjective = "Obj" };
        var events = new List<ResearchEvent>();
        await foreach (var ev in client.ResearchStreamAsync(finding)) events.Add(ev);
        Assert.Single(events);
        Assert.Equal("incomplete", events[0].Status);
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
