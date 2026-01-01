using MultiAgentCoder.Domain.Models;

namespace MultiAgentCoder.Agents.CoreAgents.Interfaces;

public interface IProjectScaffoldingAgent : IBaseScaffholdingAgent
{
    Task<CodeArtifact> EnsureBuildable(ProjectSpec projectContext, CodeArtifact artifact);
    Task<CodeArtifact> EnsureProgramCsSetup(ProjectSpec projectContext, CodeArtifact artifact);
}