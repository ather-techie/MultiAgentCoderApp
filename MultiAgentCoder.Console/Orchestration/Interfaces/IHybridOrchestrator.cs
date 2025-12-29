using MultiAgentCoder.Domain.Models;

namespace MultiAgentCoder.Console.Orchestration.Interfaces
{
    public interface IHybridOrchestrator
    {
        Task<WorkflowResult> ExecuteAsync(string problemStatement, CancellationToken cancellationToken = default);
    }
}