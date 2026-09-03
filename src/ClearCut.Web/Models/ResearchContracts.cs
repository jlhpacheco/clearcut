using System.Text.Json.Serialization;

namespace ClearCut.Web.Models;

public class EvidenceSource
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("publisher")]
    public string Publisher { get; set; } = string.Empty;

    [JsonPropertyName("retrieval_date")]
    public string RetrievalDate { get; set; } = string.Empty;

    [JsonPropertyName("relevance_summary")]
    public string RelevanceSummary { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

public class ResearchEvent
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // preparing, searching, reviewing, ready, incomplete

    [JsonPropertyName("task")]
    public string? Task { get; set; }

    [JsonPropertyName("evidence")]
    public List<EvidenceSource>? Evidence { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
