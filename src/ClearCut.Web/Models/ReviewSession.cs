namespace ClearCut.Web.Models;

public class ReviewSession
{
    public List<ReviewFinding> Findings { get; set; } = new();
    
    // Maps FindingId to list of evidence sources retrieved
    public Dictionary<string, List<EvidenceSource>> Evidence { get; set; } = new();

    // Maps FindingId to user disposition
    public Dictionary<string, Disposition?> Dispositions { get; set; } = new();

    // Maps FindingId to reviewer's manual notes
    public Dictionary<string, string> ReviewerNotes { get; set; } = new();

    // Maps FindingId to research status (e.g., "pending", "preparing", "searching", "reviewing", "ready", "incomplete")
    public Dictionary<string, string> ResearchStatus { get; set; } = new();

    // Maps FindingId to the research task/query formulated by the agent
    public Dictionary<string, string> ResearchTasks { get; set; } = new();
    
    public bool IsAnalysisActive { get; set; }
    public bool IsAnalysisComplete { get; set; }
    public string? AnalysisError { get; set; }
    
    // The finding ID currently undergoing live research, if any
    public string? ActiveResearchFindingId { get; set; }

    // Maps FindingId to its complete research trace metadata
    public Dictionary<string, ResearchTrace> ResearchTraces { get; set; } = new();

    public void Reset()
    {
        Findings.Clear();
        Evidence.Clear();
        Dispositions.Clear();
        ReviewerNotes.Clear();
        ResearchStatus.Clear();
        ResearchTasks.Clear();
        IsAnalysisActive = false;
        IsAnalysisComplete = false;
        AnalysisError = null;
        ActiveResearchFindingId = null;
        ResearchTraces.Clear();
    }
}
