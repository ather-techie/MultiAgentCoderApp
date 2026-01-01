using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiAgentCoder.Console.Cli.Commands;
using MultiAgentCoder.Console.Cli.Interfaces;
using System;

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

    public async Task RouteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();
            return;
        }

        await (args[0].ToLowerInvariant() switch
        {
            // macode generate code | tests
            "generate" => _generateCommand
                .ExecuteAsync(args.Skip(1).ToArray()),

            // macode run tests | build
            "run" => _runCommand
                .ExecuteAsync(args.Skip(1).ToArray()),

            // future commands
            // "fix" => _fixCommand.ExecuteAsync(args.Skip(1).ToArray()),
            // "scaffold" => _scaffoldCommand.ExecuteAsync(args.Skip(1).ToArray()),

            _ => UnknownCommand()
        });
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

    private Task UnknownCommand()
    {
        _logger.LogError("Unknown command");
        return Task.CompletedTask;
    }
}
