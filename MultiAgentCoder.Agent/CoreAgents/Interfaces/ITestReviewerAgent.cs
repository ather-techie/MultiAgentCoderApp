namespace MultiAgentCoder.Agents.CoreAgents.Interfaces;

public interface ITestReviewerAgent
{
    Task<string> ReviewAsync(string code, string utcode, CancellationToken ct = default);
}