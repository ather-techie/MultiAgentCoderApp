namespace MultiAgentCoder.Agents.Services.Interfaces
{
    public interface IAIOutputCleanerService
    {
        string RemoveMarkdownCSharpFence(string? content);
        string RemoveMarkdownJsonFence(string input);
    }
}