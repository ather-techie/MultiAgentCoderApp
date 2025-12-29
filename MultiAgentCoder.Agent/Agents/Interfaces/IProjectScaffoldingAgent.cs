using MultiAgentCoder.Domain.Models;

namespace MultiAgentCoder.Agents.Agents.Interfaces
{
    public interface IProjectScaffoldingAgent : IBaseScaffholdingAgent
    {
        Task<CodeArtifact> EnsureBuildable(ProjectContext projectContext, CodeArtifact artifact);
        Task<CodeArtifact> EnsureProgramCsSetup(ProjectContext projectContext, CodeArtifact artifact);
    }
}