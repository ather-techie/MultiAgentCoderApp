using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiAgentCoder.Console.Cli.Commands;
using MultiAgentCoder.Console.Cli.Interfaces;
using MultiAgentCoder.Domain.Models;
using System;
using static Google.Api.Gax.Grpc.Gcp.AffinityConfig.Types;

namespace MultiAgentCoder.Console.Cli;

public sealed class CliRouter : ICliRouter
{
    private readonly IGenerateCommand _generateCommand;
    private readonly IRunCommand _runCommand;
    private readonly ILogger<CliRouter> _logger;

    public CliRouter(IGenerateCommand generateCommand, IRunCommand runCommand,ILogger<CliRouter> logger)
    {
        _generateCommand = generateCommand;
        _runCommand = runCommand;
        _logger = logger;
    }

    public async Task<WorkflowResult> RouteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();

            return new WorkflowResult
            {
                Success = true,
                ErrorDetails = "Help displayed"
            };
        }

        var command = args[0].ToLowerInvariant();
        var remainingArgs = args.Skip(1).ToArray();

        switch (command)
        {
            case "generate":
                return await _generateCommand.ExecuteAsync(remainingArgs);

            case "run":
                return await _runCommand.ExecuteAsync(remainingArgs);

            // future commands
            // case "fix":
            //     return await _fixCommand.ExecuteAsync(remainingArgs);

            // case "scaffold":
            //     return await _scaffoldCommand.ExecuteAsync(remainingArgs);

            default:
                return UnknownCommand();
        }
    }



    private void PrintHelp()
    {
        _logger.LogInformation("""
        MultiAgentCoder CLI

        Commands:
          generate code|tests
          run tests|full
          fix code|tests
          scaffold project
        """);
    }

    private WorkflowResult UnknownCommand()
    {
        _logger.LogError("Unknown command");
        
        return new WorkflowResult
        {
            Success = false,
            ErrorDetails = "Unknown command. Use 'generate', 'run', 'fix', or 'scaffold'."
        };
    }
}
