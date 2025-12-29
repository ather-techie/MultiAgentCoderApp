using MultiAgentCoder.Agents.Agents.Interfaces;
using MultiAgentCoder.Domain.Models.Base;
using MultiAgentCoder.Domain.Models.Results;
using System.Diagnostics;

public sealed class BuildAgent : IBuildAgent
{
    public async Task<BuildResult> BuildAsync(
        BaseCodeArtifacts artifact,
        string ? msBuildArguments = null,
        CancellationToken cancellationToken = default)
    {

        var process = CreateBuildProcess(artifact.WorkingDirectory);
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync(cancellationToken);

        return new BuildResult
        {
            IsSuccess = process.ExitCode == 0,
            BuildOutput = output,
            Errors = ParseErrors(error)
        };
    }

    private static Process CreateBuildProcess(string workingDirectory)
       => new()
       {
           StartInfo = new ProcessStartInfo
           {
               FileName = "dotnet",
               Arguments = "build",
               WorkingDirectory = workingDirectory,
               RedirectStandardOutput = true,
               RedirectStandardError = true,
               CreateNoWindow = true
           }
       };

    private static IReadOnlyList<string> ParseErrors(string stderr)
        => stderr
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .ToArray();
}
