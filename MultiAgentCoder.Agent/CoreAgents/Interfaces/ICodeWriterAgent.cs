using MultiAgentCoder.Domain.Models;

namespace MultiAgentCoder.Agents.CoreAgents.Interfaces;

public interface ICodeWriterAgent
{
    Task<CodeArtifact> GenerateCodeAsync(ProjectSpec projectContext, CodeArtifact artifact, string? feedback, CancellationToken cancellationToken = default);
}