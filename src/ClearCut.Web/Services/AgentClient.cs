using System.Runtime.CompilerServices;
using System.Net.Http.Json;
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
    private readonly string _sessionId = Guid.NewGuid().ToString();

    private static readonly HashSet<string> AllowedStatuses = new() { "preparing", "searching", "reviewing", "ready", "incomplete" };

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

    public async IAsyncEnumerable<ResearchEvent> ResearchStreamAsync(ReviewFinding finding, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (finding == null) throw new ArgumentNullException(nameof(finding));
        if (string.IsNullOrWhiteSpace(finding.FindingId)) throw new ArgumentException("FindingId cannot be null or empty.", nameof(finding));
        if (string.IsNullOrWhiteSpace(finding.Label)) throw new ArgumentException("Label cannot be null or empty.", nameof(finding));
        if (string.IsNullOrWhiteSpace(finding.Observation)) throw new ArgumentException("Observation cannot be null or empty.", nameof(finding));
        if (string.IsNullOrWhiteSpace(finding.ResearchObjective)) throw new ArgumentException("ResearchObjective cannot be null or empty.", nameof(finding));

        if (_useFixtures)
        {
            // Yield realistic agent states in sequence
            var findingId = finding.FindingId;
            var objective = finding.ResearchObjective;

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
            var queries = findingId switch
            {
                "find-01-brand" => new List<string> { "LumaLeaf Energy trademark", "LumaLeaf leaf logo" },
                "find-02-claim" => new List<string> { "LumaLeaf 76% energy efficiency study" },
                "find-03-music" => new List<string> { "Cinematic Ambient Synth Track audio fingerprint" },
                _ => new List<string> { $"search for {finding.Label}" }
            };

            yield return new ResearchEvent
            {
                Status = "ready",
                Task = $"Formulated research task: {objective}",
                Evidence = evidenceList,
                SessionId = "session-fixture-123",
                SearchId = "search-fixture-456",
                RetrievalTime = DateTime.UtcNow.ToString("o"),
                Objective = objective,
                Queries = queries
            };
            yield break;
        }

        var requestBody = new
        {
            finding_id = finding.FindingId,
            label = finding.Label,
            observation = finding.Observation,
            research_objective = finding.ResearchObjective,
            session_id = _sessionId
        };

        var response = await _httpClient.PostAsJsonAsync($"{_agentBaseUrl}/v1/research/stream", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        bool receivedTerminal = false;
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
                if (string.IsNullOrWhiteSpace(researchEvent.Status) || !AllowedStatuses.Contains(researchEvent.Status))
                {
                    yield return new ResearchEvent { Status = "incomplete", Error = "Unknown or blank status received." };
                    yield break;
                }

                if (researchEvent.Status == "ready" && (researchEvent.Evidence == null || !researchEvent.Evidence.Any()))
                {
                    yield return new ResearchEvent { Status = "incomplete", Error = "Ready event received with zero evidence." };
                    yield break;
                }

                yield return researchEvent;
                if (researchEvent.Status == "ready" || researchEvent.Status == "incomplete")
                {
                    receivedTerminal = true;
                    yield break;
                }
            }
            else
            {
                yield return new ResearchEvent { Status = "incomplete", Error = "Null JSON event received." };
                yield break;
            }
        }
        if (!receivedTerminal)
        {
            yield return new ResearchEvent { Status = "incomplete", Error = "Stream ended prematurely without a terminal event." };
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
        var todayStr = DateTime.UtcNow.ToString("yyyy-MM-dd");
        return findingId switch
        {
            "find-01-brand" => new List<EvidenceSource>
            {
                new()
                {
                    Title = "United States Patent and Trademark Office TESS Database",
                    Publisher = "uspto.gov",
                    RetrievalDate = todayStr,
                    RelevanceSummary = "Fixture demonstration—no search executed. Serves as an official starting point for human search of brand marks. No final clearance or ownership conclusion is represented here.",
                    Url = "https://www.uspto.gov/trademarks"
                },
                new()
                {
                    Title = "Global Brand Database",
                    Publisher = "wipo.int",
                    RetrievalDate = todayStr,
                    RelevanceSummary = "Fixture demonstration—no search executed. Used as an official starting point for human search of international brand marks. No final clearance or ownership conclusion is represented here.",
                    Url = "https://www.wipo.int/reference/en/branddb/"
                }
            },
            "find-02-claim" => new List<EvidenceSource>
            {
                new()
                {
                    Title = "LumaLeaf Fictional Energy Study Page",
                    Publisher = "clearcut.web",
                    RetrievalDate = todayStr,
                    RelevanceSummary = "Fixture demonstration—no search executed. Contains the unique verification token CC-EVID-9F4D. This is an explicitly fictional token and claim for demonstration purposes.",
                    Url = "http://localhost:5000/evidence/lumaleaf-energy-study"
                }
            },
            "find-03-music" => new List<EvidenceSource>
            {
                new()
                {
                    Title = "APM Music Search and Licensing",
                    Publisher = "apmmusic.com",
                    RetrievalDate = todayStr,
                    RelevanceSummary = "Fixture demonstration—no search executed. Represents possible catalog research for music. No final licensing or clearance conclusion is represented here.",
                    Url = "https://www.apmmusic.com"
                },
                new()
                {
                    Title = "Shazam Audio Fingerprinting Service",
                    Publisher = "shazam.com",
                    RetrievalDate = todayStr,
                    RelevanceSummary = "Fixture demonstration—no search executed. Represents possible audio fingerprinting research for music. No final licensing or clearance conclusion is represented here.",
                    Url = "https://www.shazam.com"
                }
            },
            _ => new List<EvidenceSource>()
        };
    }
}
