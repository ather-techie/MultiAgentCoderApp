namespace MultiAgentCoder.Agents.CoreAgents.Interfaces;

public interface ICodeReviewerAgent
{
    Task<string> ReviewAsync(string code, CancellationToken ct = default);
}