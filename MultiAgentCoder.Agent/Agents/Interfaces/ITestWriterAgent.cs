using MultiAgentCoder.Domain.Models;

namespace MultiAgentCoder.Agents.Agents.Interfaces
{
    public interface ITestWriterAgent
    {
        Task<UnitTestCodeArtifacts> GenerateTestsAsync(ProjectContext projectContext, CodeArtifact artifact, CancellationToken cancellationToken = default);
    }
}