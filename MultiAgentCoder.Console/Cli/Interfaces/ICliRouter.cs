using MultiAgentCoder.Domain.Models;

namespace MultiAgentCoder.Console.Cli.Interfaces
{
    public interface ICliRouter
    {
        Task<WorkflowResult> RouteAsync(string[] args);
    }
}