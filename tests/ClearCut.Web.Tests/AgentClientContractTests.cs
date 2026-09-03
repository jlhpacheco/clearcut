using System.Text.Json;
using Xunit;
using ClearCut.Web.Models;

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
}
