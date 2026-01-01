using MultiAgentCoder.Console.Orchestration;
using MultiAgentCoder.Domain.Models;

namespace MultiAgentCoder.Agents.RoleAgents.Interfaces
{
    public interface IDevAgent
    {
        Task<WorkflowResult> ExecuteAsync(WorkflowContext workflowContext, ProjectSpec project, CancellationToken cancellationToken = default);
    }
}