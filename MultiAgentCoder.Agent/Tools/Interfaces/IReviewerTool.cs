namespace MultiAgentCoder.Agents.Tools.Interfaces
{
    public interface IReviewerTool
    {
        Task<string> ReviewAsync(string code);
    }
}