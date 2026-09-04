using ClearCut.Web.Models;

namespace ClearCut.Web.Services;

public class ReviewSessionStore
{
    private readonly AgentClient _agentClient;
    private readonly ReviewSession _session = new();

    public event Action? OnChange;

    public ReviewSession Session => _session;

    public ReviewSessionStore(AgentClient agentClient)
    {
        _agentClient = agentClient;
    }

    public bool IsOperationActive => _session.IsAnalysisActive || _session.ActiveResearchFindingId != null;

    public async Task BeginAnalysisAsync()
    {
        if (IsOperationActive)
        {
            throw new InvalidOperationException("An operation is already in progress.");
        }

        _session.Reset();
        _session.IsAnalysisActive = true;
        _session.AnalysisError = null;
        NotifyStateChanged();

        try
        {
            var response = await _agentClient.AnalyzeAsync();
            if (response.Findings == null || response.Findings.Count == 0)
            {
                _session.AnalysisError = "Analysis incomplete: No findings returned from analyzer.";
            }
            else if (response.Findings.Count < 3)
            {
                _session.AnalysisError = "Analysis incomplete: Fewer than three findings returned.";
            }
            else
            {
                // Ensure chronological sort
                _session.Findings = response.Findings.OrderBy(f => f.StartSeconds).ToList();
                foreach (var f in _session.Findings)
                {
                    _session.ResearchStatus[f.FindingId] = "pending";
                    _session.Dispositions[f.FindingId] = null;
                    _session.ReviewerNotes[f.FindingId] = string.Empty;
                }
                _session.IsAnalysisComplete = true;
            }
        }
        catch (Exception ex)
        {
            _session.AnalysisError = $"Analysis unavailable: {ex.Message}";
        }
        finally
        {
            _session.IsAnalysisActive = false;
            NotifyStateChanged();
        }
    }

    public async Task BeginResearchAsync(string findingId)
    {
        if (IsOperationActive)
        {
            throw new InvalidOperationException("An operation is already in progress.");
        }

        var finding = _session.Findings.FirstOrDefault(f => f.FindingId == findingId);
        if (finding == null)
        {
            throw new ArgumentException("Finding not found in current session.", nameof(findingId));
        }

        _session.ActiveResearchFindingId = findingId;
        _session.ResearchStatus[findingId] = "preparing";
        NotifyStateChanged();

        try
        {
            await foreach (var ev in _agentClient.ResearchStreamAsync(finding))
            {
                if (ev.Status == "preparing" || ev.Status == "searching" || ev.Status == "reviewing")
                {
                    _session.ResearchStatus[findingId] = ev.Status;
                    if (!string.IsNullOrEmpty(ev.Task))
                    {
                        _session.ResearchTasks[findingId] = ev.Task;
                    }
                }
                else if (ev.Status == "ready")
                {
                    var hasEvidence = ev.Evidence != null && ev.Evidence.Any();
                    var hasTrace = !string.IsNullOrWhiteSpace(ev.Objective) &&
                                   !string.IsNullOrWhiteSpace(ev.SessionId) &&
                                   !string.IsNullOrWhiteSpace(ev.SearchId) &&
                                   !string.IsNullOrWhiteSpace(ev.RetrievalTime);
                    var cleanQueries = ev.Queries?.Where(q => !string.IsNullOrWhiteSpace(q)).ToList() ?? new List<string>();
                    var hasValidQueries = cleanQueries.Count >= 1 && cleanQueries.Count <= 3;
                    if (hasEvidence && hasTrace && hasValidQueries)
                    {
                        _session.ResearchStatus[findingId] = "ready";
                        _session.Evidence[findingId] = ev.Evidence!;
                        _session.ResearchTraces[findingId] = new ResearchTrace
                        {
                            FindingId = findingId,
                            Objective = ev.Objective!,
                            Queries = cleanQueries,
                            SessionId = ev.SessionId!,
                            SearchId = ev.SearchId!,
                            RetrievalTime = ev.RetrievalTime!
                        };
                    }
                    else
                    {
                        _session.ResearchStatus[findingId] = "incomplete";
                    }
                }
                else if (ev.Status == "incomplete")
                {
                    _session.ResearchStatus[findingId] = "incomplete";
                    // PRD story 4.4: "If prior successful evidence exists and a retry fails, the prior evidence remains visible and is labeled as the previous successful result."
                    // We preserve the existing evidence dictionary for findingId, but set status to incomplete.
                }

                NotifyStateChanged();
            }
        }
        catch (Exception)
        {
            _session.ResearchStatus[findingId] = "incomplete";
            NotifyStateChanged();
        }
        finally
        {
            _session.ActiveResearchFindingId = null;
            NotifyStateChanged();
        }
    }

    public bool CanDismiss(string findingId)
    {
        // Dismiss requires successful evidence (status == ready and non-empty evidence list)
        return _session.ResearchStatus.TryGetValue(findingId, out var status) &&
               status == "ready" &&
               _session.Evidence.TryGetValue(findingId, out var evidence) &&
               evidence.Count > 0;
    }

    public void SetDisposition(string findingId, Disposition? disposition)
    {
        if (disposition == Disposition.Dismiss && !CanDismiss(findingId))
        {
            throw new InvalidOperationException("Dismiss is unavailable. Evidence review is required.");
        }

        _session.Dispositions[findingId] = disposition;
        NotifyStateChanged();
    }

    public void SetReviewerNote(string findingId, string note)
    {
        _session.ReviewerNotes[findingId] = note;
        NotifyStateChanged();
    }

    public bool CanPrint()
    {
        // Printable export is only available when all findings (exactly three required) have a selected human disposition.
        if (_session.Findings.Count == 0 || !_session.IsAnalysisComplete) return false;

        foreach (var finding in _session.Findings)
        {
            if (!_session.Dispositions.TryGetValue(finding.FindingId, out var disp) || disp == null)
            {
                return false;
            }
        }
        return true;
    }

    public void Reset()
    {
        _session.Reset();
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
