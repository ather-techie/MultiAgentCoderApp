using MultiAgentCoder.Domain.Models;
using MultiAgentCoder.Domain.Models.Results;

namespace MultiAgentCoder.Agents.Agents.Interfaces
{
    public interface ITestRunnerAgent
    {
        Task<TestRunResult> RunAsync(ProjectContext projectContext, UnitTestCodeArtifacts testArtifact, CancellationToken cancellationToken = default);
    }
}