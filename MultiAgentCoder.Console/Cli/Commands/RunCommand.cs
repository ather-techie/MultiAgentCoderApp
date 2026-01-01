using Microsoft.Extensions.Logging;
using MultiAgentCoder.Agents.CoreAgents.Interfaces;
using MultiAgentCoder.Agents.CoreAgents.Tests;
using MultiAgentCoder.Console.Cli.Interfaces;
using MultiAgentCoder.Console.Orchestration;
using MultiAgentCoder.Console.Orchestration.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAgentCoder.Console.Cli.Commands;

public sealed class RunCommand : IRunCommand
{
    private readonly IHybridOrchestrator _orchestrator;
    private readonly ITestRunnerAgent _testRunner;
    private readonly ILogger<RunCommand> _logger;

    public RunCommand(
        IHybridOrchestrator orchestrator,
        ITestRunnerAgent testRunner,
        ILogger<RunCommand> logger)
    {
        _orchestrator = orchestrator;
        _testRunner = testRunner;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        return args.FirstOrDefault() switch
        {
           // "tests" => await RunTests(),
           // "full" => await RunFull(),
            _ => Fail("Specify 'tests' or 'full'")
        };
    }

    //private async Task<int> RunTests()
    //{
    //    //var result = await _testRunner.ExecuteAsync();
    //    //return result.IsSuccess ? 0 : 1;
    //}

    //private async Task<int> RunFull()
    //{
    //    ;
    //    //var result = await _orchestrator.RunAsync();
    //    //return result.IsSuccess ? 0 : 1;
    //}

    private  int Fail(string msg)
    {
        _logger.LogError(msg);
        return 1;
    }
}
