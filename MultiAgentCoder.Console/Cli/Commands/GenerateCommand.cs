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
        HybridOrchestrator hybridOrchestrator,
        ILogger<GenerateCommand> logger)
    {
        _devAgent = devAgent;
        _qaAgent = qaAgent;
        _hybridOrchestrator = hybridOrchestrator;
        _logger = logger;
    }

    public async Task ExecuteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            // or: return Fail("Specify 'code' or 'tests'");
            return;
        }

        var spec = new ProjectSpec().Fill(args.Skip(1).ToArray());
        var context = new WorkflowContext(spec.ProblemStatement);

        await (args[0].ToLowerInvariant() switch
        {
            "code" => _devAgent.ExecuteAsync(context, spec),

            "tests" => _qaAgent.ExecuteAsync(context, spec),

            "full" => _hybridOrchestrator.ExecuteAsync(spec),

            _ => FailAsync("Invalid generate option")
        });
    }


    //private async Task<int> GenerateCode()
    //{
    //    var result = await _codeWriter.GenerateCodeAsync()
    //    return PrintResult(result);
    //}

    //private async Task<int> GenerateTests()
    //{
    //    var result = await _testWriter.ExecuteAsync();
    //    return PrintResult(result);
    //}

    //private static int PrintResult(AgentResult result)
    //{
    //    Console.WriteLine(result.IsSuccess
    //        ? "✔ Success"
    //        : "✖ Failed");

    //    return result.IsSuccess ? 0 : 1;
    //}

    private Task FailAsync(string message)
    {
        _logger.LogError(message);
        return Task.CompletedTask;
    }
}
