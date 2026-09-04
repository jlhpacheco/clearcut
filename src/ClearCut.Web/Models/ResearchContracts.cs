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

    [JsonPropertyName("queries")]
    public List<string>? Queries { get; set; }

    [JsonPropertyName("objective")]
    public string? Objective { get; set; }

    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }

    [JsonPropertyName("search_id")]
    public string? SearchId { get; set; }

    [JsonPropertyName("retrieval_time")]
    public string? RetrievalTime { get; set; }
}

public class ResearchTrace
{
    public string FindingId { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public List<string> Queries { get; set; } = new();
    public string SessionId { get; set; } = string.Empty;
    public string SearchId { get; set; } = string.Empty;
    public string RetrievalTime { get; set; } = string.Empty;
}
