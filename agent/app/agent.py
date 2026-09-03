import contextvars
import datetime
import logging
from typing import List, Dict, Any, Optional
from google.adk.agents import Agent
from app.contracts import EvidenceSource
from app.parallel_search import execute_parallel_search, get_fixture_evidence
from app.settings import settings

logger = logging.getLogger(__name__)

# ContextVars to manage thread/async-safe tool execution tracking
tool_results_var = contextvars.ContextVar("tool_results", default=None)
tool_calls_count_var = contextvars.ContextVar("tool_calls_count", default=0)

# Proper type-annotated ADK Tool
async def parallel_search_tool(
    queries: List[str],
    objective: str,
    session_id: str
) -> List[Dict[str, Any]]:
    """Execute a parallelized web search across multiple sources for film clearance.
    
    Args:
        queries (List[str]): A list of 1 to 3 search queries to execute on the Parallel Search API.
        objective (str): The natural language research objective for the clearance preparation.
        session_id (str): A unique session ID for grouping related requests.
        
    Returns:
        List[Dict[str, Any]]: A list of dictionary-serialized EvidenceSource objects.
    """
    # Enforce single-call/no-retry policy
    count = tool_calls_count_var.get()
    if count >= 1:
        raise RuntimeError("Tool execution policy violation: parallel_search_tool called more than once.")
    tool_calls_count_var.set(count + 1)

    # Delegate to the execute_parallel_search implementation
    res_dict = await execute_parallel_search(
        queries=queries,
        objective=objective,
        session_id=session_id
    )
    
    # Store the actual queries formulated by the Agent inside the results dict
    res_dict["queries"] = queries
    
    # Save in ContextVar to return up to run_research
    tool_results_var.set(res_dict)
    
    return [ev.model_dump() for ev in res_dict["evidence"]]

# Instantiate actual native Google ADK root Agent with exactly one typed Parallel search function tool
root_agent = Agent(
    name="clear_cut_research_agent",
    model=settings.GEMINI_MODEL,
    instruction=(
        "You are a clearance-preparation research agent. "
        "Formulate 1 to 3 concise search queries and execute the parallel_search_tool exactly once. "
        "Do not synthesize URLs. Highlight conflicts or uncertainty, but do NOT make legal conclusions or select dispositions."
    ),
    tools=[parallel_search_tool]
)

class ResearchAgent:
    """
    ClearCut Private Agent wrapper coordinating Google ADK agent.
    """
    def __init__(self):
        self.root_agent = root_agent

    def get_registered_tools(self) -> List[str]:
        # Return registered tool names
        return [tool.__name__ for tool in self.root_agent.tools]

    async def run_research(self, finding_id: str, objective: str, session_id: str) -> Dict[str, Any]:
        """
        Runs the research workflow. If USE_FIXTURES is enabled, returns fixture evidence.
        Otherwise, runs actual Google ADK execution where Gemini derives the objective and queries
        and calls the parallel search tool exactly once.
        """
        if settings.USE_FIXTURES:
            # Fixture mode is visibly and structurally separate and deterministic
            evidence = get_fixture_evidence(finding_id)
            search_id = f"srch_fix_{finding_id}"
            retrieval_time = datetime.datetime.now(datetime.timezone.utc).isoformat()
            
            if finding_id == "find-01-brand":
                queries = ["LumaLeaf Energy trademark", "LumaLeaf stylized leaf logo registry"]
            elif finding_id == "find-02-claim":
                queries = ["LumaLeaf 76 percent energy saving", "LumaLeaf Energy scientific study"]
            elif finding_id == "find-03-music":
                queries = ["cinematic ambient background synth cue apm", "electronic background track shazam"]
            else:
                queries = [f"{objective} verification"]
                
            return {
                "objective": f"Research clearance for: {objective}",
                "queries": queries,
                "evidence": evidence,
                "session_id": session_id,
                "search_id": search_id,
                "retrieval_time": retrieval_time
            }

        # Real/Runtime mode: Actual Google ADK execution
        # Re-initialize ContextVars
        tool_results_var.set(None)
        tool_calls_count_var.set(0)
        
        from google.adk.runners import InMemoryRunner
        runner = InMemoryRunner(agent=self.root_agent)
        
        prompt = (
            f"Please research the following finding for film clearance.\n"
            f"Finding ID: {finding_id}\n"
            f"Observation and objective: {objective}\n"
            f"Session ID: {session_id}\n\n"
            f"Determine the research objective and derive 1 to 3 search queries to verify this finding. "
            f"Then, invoke the parallel_search_tool exactly once using the queries, the objective, and the Session ID."
        )
        
        # Execute the agent using InMemoryRunner
        await runner.run_debug(prompt)
        
        # Retrieve results from ContextVar
        res_dict = tool_results_var.get()
        if not res_dict:
            raise RuntimeError("Google ADK execution failed to invoke parallel_search_tool.")
            
        evidence = res_dict["evidence"]
        search_id = res_dict["search_id"]
        retrieval_time = res_dict["retrieval_time"]
        queries = res_dict.get("queries", [f"{objective} verification"])
        
        return {
            "objective": f"Research clearance for: {objective}",
            "queries": queries,
            "evidence": evidence,
            "session_id": session_id,
            "search_id": search_id,
            "retrieval_time": retrieval_time
        }
