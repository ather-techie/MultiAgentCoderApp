using MultiAgentCoder.Domain.Models;

namespace MultiAgentCoder.Agents.CoreAgents.Interfaces;

public interface ITestWriterAgent
{
    Task<UnitTestCodeArtifacts> GenerateTestsAsync(ProjectSpec projectContext, CodeArtifact artifact, CancellationToken cancellationToken = default);
}