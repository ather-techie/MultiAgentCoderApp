using MultiAgentCoder.Domain.Models;

namespace MultiAgentCoder.Console.Cli.Interfaces
{
    public interface IRunCommand
    {
        Task<WorkflowResult> ExecuteAsync(string[] args);
    }
}