namespace MultiAgentCoder.Agents.Agents.Interfaces
{
    public interface IReviewerAgent
    {
        Task<string> ReviewAsync(string code, CancellationToken ct = default);
    }
}