using Microsoft.Extensions.Logging;
using MultiAgentCoder.Agents.Agents.Interfaces;
using MultiAgentCoder.Agents.Services;
using MultiAgentCoder.Agents.Services.Interfaces;
using MultiAgentCoder.Domain.Models;
using MultiAgentCoder.Domain.Models.Results;
using System.Diagnostics;

namespace MultiAgentCoder.Agents.Agents.Tests;

public sealed class TestRunnerAgent : ITestRunnerAgent
{
    private readonly IFileService _fileService;
    private readonly ILogger<TestRunnerAgent> _logger;

    public TestRunnerAgent(
        IFileService fileService,
        ILogger<TestRunnerAgent> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<TestRunResult> RunAsync(
        ProjectContext projectContext,
        UnitTestCodeArtifacts testArtifact,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testArtifact);

        var (exists, testProjectDir) = _fileService.DirectoryExists(projectContext, testArtifact);

        if (!exists)
        {
            return new TestRunResult
            {
                IsSuccess = false,
                Errors = [$"Test project directory does not exist for artifact: {testProjectDir}"]
            };
        }

        var stopwatch = Stopwatch.StartNew();

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "test --nologo --verbosity quiet",
                WorkingDirectory = testProjectDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        _logger.LogInformation("Running tests in {Dir}", testProjectDir);

        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        stopwatch.Stop();

        var output = await outputTask;
        var error = await errorTask;

        var errors = string.IsNullOrWhiteSpace(error)
            ? []
            : error.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        return new TestRunResult
        {
            IsSuccess = process.ExitCode == 0,
            Output = output,
            Errors = errors,
            Duration = stopwatch.Elapsed
        };
    }
}
