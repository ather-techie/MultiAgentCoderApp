using MultiAgentCoder.Domain.Models;

namespace MultiAgentCoder.Console.Cli.Interfaces
{
    public interface IGenerateCommand
    {
        Task<WorkflowResult> ExecuteAsync(string[] args);
    }
}