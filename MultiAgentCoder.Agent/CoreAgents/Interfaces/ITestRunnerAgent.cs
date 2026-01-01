using MultiAgentCoder.Domain.Models;
using MultiAgentCoder.Domain.Models.Results;

namespace MultiAgentCoder.Agents.CoreAgents.Interfaces;

public interface ITestRunnerAgent
{
    Task<TestRunResult> RunAsync(ProjectSpec projectContext, UnitTestCodeArtifacts testArtifact, CancellationToken cancellationToken = default);
}