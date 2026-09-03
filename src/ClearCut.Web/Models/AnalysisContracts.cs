using System.Text.Json.Serialization;

namespace ClearCut.Web.Models;

public class ReviewFinding
{
    [JsonPropertyName("finding_id")]
    public string FindingId { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty; // brand_mark, factual_claim, music_cue

    [JsonPropertyName("start_seconds")]
    public double StartSeconds { get; set; }

    [JsonPropertyName("end_seconds")]
    public double? EndSeconds { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("observation")]
    public string Observation { get; set; } = string.Empty;

    [JsonPropertyName("review_priority")]
    public string ReviewPriority { get; set; } = string.Empty; // routine, attention, priority

    [JsonPropertyName("research_objective")]
    public string ResearchObjective { get; set; } = string.Empty;

    public string GetCategoryDisplayName()
    {
        return Category switch
        {
            "brand_mark" => "Brand Mark",
            "factual_claim" => "Factual Claim",
            "music_cue" => "Music Cue",
            _ => Category
        };
    }

    public string GetPriorityDisplayName()
    {
        return ReviewPriority switch
        {
            "routine" => "Routine Review",
            "attention" => "Requires Attention",
            "priority" => "Priority Check",
            _ => ReviewPriority
        };
    }
}

public class AnalysisResponse
{
    [JsonPropertyName("findings")]
    public List<ReviewFinding> Findings { get; set; } = new();
}
