using ClearCut.Web.Models;

namespace ClearCut.Web.Services;

public class ReportService
{
    public class ReportModel
    {
        public string ProjectName { get; set; } = "ClearCut Demonstration Rough Cut";
        public string ExportDate { get; set; } = string.Empty;
        public string Disclaimer { get; set; } = "ClearCut is an automated clearance-preparation research assistant. The information provided is for research purposes only and does not constitute legal advice, a binding clearance determination, or a guarantee of errors-and-omissions eligibility.";
        public List<ReportItem> Items { get; set; } = new();
    }

    public class ReportItem
    {
        public string FindingId { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Timeframe { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Observation { get; set; } = string.Empty;
        public string ResearchStatus { get; set; } = string.Empty;
        public int SourceCount { get; set; }
        public string Disposition { get; set; } = "Pending Review";
        public string ReviewerNote { get; set; } = string.Empty;
        public List<EvidenceSource> Sources { get; set; } = new();
    }

    public ReportModel GenerateReport(ReviewSession session)
    {
        var model = new ReportModel
        {
            ExportDate = DateTime.Now.ToString("MMMM dd, yyyy")
        };

        foreach (var finding in session.Findings)
        {
            session.Evidence.TryGetValue(finding.FindingId, out var sources);
            session.Dispositions.TryGetValue(finding.FindingId, out var disp);
            session.ReviewerNotes.TryGetValue(finding.FindingId, out var note);
            session.ResearchStatus.TryGetValue(finding.FindingId, out var status);

            var item = new ReportItem
            {
                FindingId = finding.FindingId,
                Category = finding.GetCategoryDisplayName(),
                Timeframe = FormatTimeframe(finding.StartSeconds, finding.EndSeconds),
                Label = finding.Label,
                Observation = finding.Observation,
                ResearchStatus = status switch
                {
                    "pending" => "Not Researched",
                    "preparing" => "Preparing Research Task",
                    "searching" => "Searching with Parallel",
                    "reviewing" => "Reviewing Sources",
                    "ready" => "Evidence Ready",
                    "incomplete" => "Evidence Incomplete",
                    _ => status ?? "Unknown"
                },
                SourceCount = sources?.Count ?? 0,
                Disposition = disp?.ToString() ?? "Pending Review",
                ReviewerNote = string.IsNullOrWhiteSpace(note) ? "No reviewer notes recorded." : note,
                Sources = sources ?? new List<EvidenceSource>()
            };

            model.Items.Add(item);
        }

        return model;
    }

    public static string FormatSeconds(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        return $"{(int)ts.TotalMinutes:00}:{ts.Seconds:00}";
    }

    public static string FormatTimeframe(double start, double? end)
    {
        if (end.HasValue)
        {
            return $"{FormatSeconds(start)} – {FormatSeconds(end.Value)}";
        }
        return FormatSeconds(start);
    }
}
