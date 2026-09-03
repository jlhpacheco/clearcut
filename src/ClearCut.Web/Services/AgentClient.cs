using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClearCut.Web.Models;

namespace ClearCut.Web.Services;

public class AgentClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly bool _useFixtures;
    private readonly string _agentBaseUrl;

    public AgentClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _useFixtures = _configuration.GetValue<bool>("UseFixtures", true);
        _agentBaseUrl = _configuration["CLEARCUT_AGENT_BASE_URL"] ?? "http://localhost:8000";
    }

    public async Task<AnalysisResponse> AnalyzeAsync(CancellationToken cancellationToken = default)
    {
        if (_useFixtures)
        {
            // Simulate networking delay
            await Task.Delay(1500, cancellationToken);
            return GetFixtureAnalysisResponse();
        }

        var response = await _httpClient.PostAsJsonAsync($"{_agentBaseUrl}/v1/analyze", new { }, cancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<AnalysisResponse>(content) ?? throw new InvalidOperationException("Analysis response was null.");
    }

    public async IAsyncEnumerable<ResearchEvent> ResearchStreamAsync(string findingId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_useFixtures)
        {
            // Yield realistic agent states in sequence
            var finding = GetFixtureAnalysisResponse().Findings.FirstOrDefault(f => f.FindingId == findingId);
            var objective = finding?.ResearchObjective ?? "Verify clearance status.";

            yield return new ResearchEvent
            {
                Status = "preparing",
                Task = $"Formulated research task: {objective}"
            };
            await Task.Delay(1000, cancellationToken);

            yield return new ResearchEvent
            {
                Status = "searching",
                Task = $"Formulated research task: {objective}"
            };
            await Task.Delay(1200, cancellationToken);

            yield return new ResearchEvent
            {
                Status = "reviewing",
                Task = $"Formulated research task: {objective}"
            };
            await Task.Delay(1000, cancellationToken);

            var evidenceList = GetFixtureEvidence(findingId);
            yield return new ResearchEvent
            {
                Status = "ready",
                Task = $"Formulated research task: {objective}",
                Evidence = evidenceList
            };
            yield break;
        }

        var response = await _httpClient.PostAsJsonAsync($"{_agentBaseUrl}/v1/research/stream", new { finding_id = findingId }, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line == null) break;
            if (string.IsNullOrWhiteSpace(line)) continue;

            ResearchEvent? researchEvent = null;
            bool isMalformed = false;
            try
            {
                researchEvent = JsonSerializer.Deserialize<ResearchEvent>(line);
            }
            catch (JsonException)
            {
                isMalformed = true;
            }

            if (isMalformed)
            {
                yield return new ResearchEvent { Status = "incomplete", Error = "Malformed event received from stream." };
                yield break;
            }

            if (researchEvent != null)
            {
                yield return researchEvent;
                if (researchEvent.Status == "ready" || researchEvent.Status == "incomplete")
                {
                    yield break;
                }
            }
        }
    }

    private static AnalysisResponse GetFixtureAnalysisResponse()
    {
        return new AnalysisResponse
        {
            Findings = new List<ReviewFinding>
            {
                new()
                {
                    FindingId = "find-01-brand",
                    Category = "brand_mark",
                    StartSeconds = 4.5,
                    EndSeconds = 12.0,
                    Label = "LumaLeaf Logo",
                    Observation = "Green stylized leaf logo displaying 'LumaLeaf Energy'. Possible trademark clearance required.",
                    ReviewPriority = "attention",
                    ResearchObjective = "Determine if 'LumaLeaf Energy' or the green leaf logo matches registered trademarks or existing brand marks."
                },
                new()
                {
                    FindingId = "find-02-claim",
                    Category = "factual_claim",
                    StartSeconds = 15.0,
                    EndSeconds = 22.5,
                    Label = "LumaLeaf 76% Energy Comparison",
                    Observation = "On-screen text and voiceover claim '76% more energy efficient than traditional sources'. LumaLeaf Energy study cited.",
                    ReviewPriority = "priority",
                    ResearchObjective = "Verify the scientific basis or public registry of the LumaLeaf Energy study claiming 76% energy efficiency."
                },
                new()
                {
                    FindingId = "find-03-music",
                    Category = "music_cue",
                    StartSeconds = 25.0,
                    EndSeconds = 38.0,
                    Label = "Cinematic Ambient Synth Track",
                    Observation = "Electronic ambient background music cue playing during the second half of the clip.",
                    ReviewPriority = "routine",
                    ResearchObjective = "Identify the origin and licensing status of the electronic ambient background synth track."
                }
            }
        };
    }

    private static List<EvidenceSource> GetFixtureEvidence(string findingId)
    {
        return findingId switch
        {
            "find-01-brand" => new List<EvidenceSource>
            {
                new()
                {
                    Title = "United States Patent and Trademark Office TESS Database",
                    Publisher = "uspto.gov",
                    RetrievalDate = "2026-09-02",
                    RelevanceSummary = "No active trademark registrations found for 'LumaLeaf' or 'LumaLeaf Energy' under class 042 (Energy).",
                    Url = "https://www.uspto.gov/trademarks"
                },
                new()
                {
                    Title = "Global Brand Database",
                    Publisher = "wipo.int",
                    RetrievalDate = "2026-09-02",
                    RelevanceSummary = "No active international trademark filings found matching 'LumaLeaf' with a stylized leaf emblem.",
                    Url = "https://www.wipo.int/reference/en/branddb/"
                }
            },
            "find-02-claim" => new List<EvidenceSource>
            {
                new()
                {
                    Title = "LumaLeaf Fictional Energy Study Page",
                    Publisher = "clearcut.web",
                    RetrievalDate = "2026-09-02",
                    RelevanceSummary = "Contains the unique verification token CC-EVID-9F4D. Explicitly states that LumaLeaf Energy and its 76% comparison claims are entirely fictional demonstration data.",
                    Url = "http://localhost:5000/evidence/lumaleaf-energy-study"
                }
            },
            "find-03-music" => new List<EvidenceSource>
            {
                new()
                {
                    Title = "APM Music Search and Licensing",
                    Publisher = "apmmusic.com",
                    RetrievalDate = "2026-09-02",
                    RelevanceSummary = "No audio matches found in the APM production music catalogs for this background track. Music cue is likely an custom-composed track.",
                    Url = "https://www.apmmusic.com"
                },
                new()
                {
                    Title = "Shazam Audio Fingerprinting Service",
                    Publisher = "shazam.com",
                    RetrievalDate = "2026-09-02",
                    RelevanceSummary = "No matches found in the commercial music catalog. Supports the finding that this is an original or unreleased composition.",
                    Url = "https://www.shazam.com"
                }
            },
            _ => new List<EvidenceSource>()
        };
    }
}
