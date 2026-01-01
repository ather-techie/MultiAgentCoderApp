using MultiAgentCoder.Domain.Models;
using MultiAgentCoder.Domain.Models.Base;

namespace MultiAgentCoder.Agents.Services.Interfaces
{
    public interface IProjectService
    {
        string CreateProjectName(ProjectSpec projectContext, BaseCodeArtifacts artifact);
        string CreateSafeNamespace(ProjectSpec projectContext, BaseCodeArtifacts artifact);
        string InferProjectName(string problemStatement);
    }
}