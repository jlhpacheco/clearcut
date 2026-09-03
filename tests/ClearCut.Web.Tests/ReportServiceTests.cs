using Xunit;
using ClearCut.Web.Models;
using ClearCut.Web.Services;

namespace ClearCut.Web.Tests;

public class ReportServiceTests
{
    private readonly ReportService _service = new();

    [Theory]
    [InlineData(0, "00:00")]
    [InlineData(4.5, "00:04")]
    [InlineData(59.9, "00:59")]
    [InlineData(60, "01:00")]
    [InlineData(125.3, "02:05")]
    public void FormatSeconds_FormatedCorrectly(double input, string expected)
    {
        var actual = ReportService.FormatSeconds(input);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(4.5, null, "00:04")]
    [InlineData(4.5, 12.0, "00:04 – 00:12")]
    public void FormatTimeframe_FormatedCorrectly(double start, double? end, string expected)
    {
        var actual = ReportService.FormatTimeframe(start, end);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GenerateReport_CreatesValidViewModel()
    {
        // Arrange
        var session = new ReviewSession();
        var finding = new ReviewFinding
        {
            FindingId = "f1",
            Category = "brand_mark",
            StartSeconds = 5.0,
            EndSeconds = 10.0,
            Label = "Test Brand",
            Observation = "A test observation"
        };
        session.Findings.Add(finding);
        session.Dispositions["f1"] = Disposition.Investigate;
        session.ReviewerNotes["f1"] = "Follow up with legal.";
        session.ResearchStatus["f1"] = "ready";
        session.Evidence["f1"] = new List<EvidenceSource>
        {
            new() { Title = "USPTO", Publisher = "gov", RetrievalDate = "2026-09-02", Url = "http://uspto.gov" }
        };

        // Act
        var report = _service.GenerateReport(session);

        // Assert
        Assert.NotNull(report);
        Assert.Single(report.Items);
        
        var item = report.Items[0];
        Assert.Equal("f1", item.FindingId);
        Assert.Equal("Brand Mark", item.Category);
        Assert.Equal("00:05 – 00:10", item.Timeframe);
        Assert.Equal("Test Brand", item.Label);
        Assert.Equal("A test observation", item.Observation);
        Assert.Equal("Evidence Ready", item.ResearchStatus);
        Assert.Equal(1, item.SourceCount);
        Assert.Equal("Investigate", item.Disposition);
        Assert.Equal("Follow up with legal.", item.ReviewerNote);
        Assert.Single(item.Sources);
    }
}
