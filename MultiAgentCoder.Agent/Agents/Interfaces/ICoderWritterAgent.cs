using MultiAgentCoder.Domain.Models;

namespace MultiAgentCoder.Agents.Agents.Interfaces
{
    public interface ICoderWritterAgent
    {
        Task<CodeArtifact> GenerateCodeAsync(ProjectContext projectContext, CodeArtifact artifact, string problemStatement, string? feedback, CancellationToken cancellationToken = default);
    }
}