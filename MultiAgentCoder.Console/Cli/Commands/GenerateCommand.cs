using Microsoft.Extensions.Logging;
using MultiAgentCoder.Agents.CoreAgents.Dev;
using MultiAgentCoder.Agents.CoreAgents.Interfaces;
using MultiAgentCoder.Agents.CoreAgents.Tests;
using MultiAgentCoder.Agents.RoleAgents;
using MultiAgentCoder.Agents.RoleAgents.Interfaces;
using MultiAgentCoder.Console.Cli.Interfaces;
using MultiAgentCoder.Console.Extensions;
using MultiAgentCoder.Console.Orchestration;
using MultiAgentCoder.Console.Orchestration.Interfaces;
using MultiAgentCoder.Domain.Models;

namespace MultiAgentCoder.Console.Cli.Commands;

public sealed class GenerateCommand : IGenerateCommand
{
    private readonly IDevAgent _devAgent;
    private readonly IQaAgent _qaAgent;
    private readonly IHybridOrchestrator _hybridOrchestrator;
    private readonly ILogger<GenerateCommand> _logger;

    public GenerateCommand(
        IDevAgent devAgent,
        IQaAgent qaAgent,
        IHybridOrchestrator hybridOrchestrator,
        ILogger<GenerateCommand> logger)
    {
        _devAgent = devAgent;
        _qaAgent = qaAgent;
        _hybridOrchestrator = hybridOrchestrator;
        _logger = logger;
    }

    public async Task<WorkflowResult> ExecuteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            // or: return Fail("Specify 'code' or 'tests'");
            return new WorkflowResult()
            {
                Success = false,
                ErrorDetails = "Specify 'code', 'tests', or 'full' as the first argument."
            };
        }

        var spec = new ProjectSpec().Fill(args.Skip(1).ToArray());
        var context = new WorkflowContext(spec.ProblemStatement);

        switch (args[0].ToLowerInvariant())
        {
            case "code":
                return await _devAgent.ExecuteAsync(context, spec);

            case "tests":
            case "test":
                return await _qaAgent.ExecuteAsync(context, spec);

            case "full":
                return await _hybridOrchestrator.ExecuteAsync(spec);

            default:
                return FailAsync("Invalid generate option");
        }
    }

    private WorkflowResult FailAsync(string message)
    {
        _logger.LogError(message);

        return new WorkflowResult()
        {
            Success = false,
            ErrorDetails = message
        };
    }
}
