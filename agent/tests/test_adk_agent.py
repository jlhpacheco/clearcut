import pytest
import contextvars
from app.agent import root_agent, parallel_search_tool, tool_calls_count_var

def test_tool_registration():
    # Prove exactly one tool registration
    assert len(root_agent.tools) == 1
    assert root_agent.tools[0].__name__ == "parallel_search_tool"

@pytest.mark.asyncio
async def test_single_call_no_retry_enforcement():
    # Enforce single-call/no-retry behavior in a clean context isolation
    ctx = contextvars.copy_context()
    
    async def run_in_context():
        # Reset count to 0 for this context
        tool_calls_count_var.set(0)
        
        # First call should succeed (it uses fixture search in test environment)
        await parallel_search_tool(queries=["test"], objective="test obj", session_id="test_sess")
        assert tool_calls_count_var.get() == 1
        
        # Second call must raise RuntimeError (enforcing single-call policy)
        with pytest.raises(RuntimeError) as exc_info:
            await parallel_search_tool(queries=["test2"], objective="test obj2", session_id="test_sess")
            
        assert "parallel_search_tool called more than once" in str(exc_info.value)

    await ctx.run(run_in_context)
