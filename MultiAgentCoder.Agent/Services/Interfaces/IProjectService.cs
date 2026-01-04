using MultiAgentCoder.Domain.Models;
using MultiAgentCoder.Domain.Models.Base;

namespace MultiAgentCoder.Agents.Services.Interfaces
{
    public interface IProjectService
    {
        string CreateSafeNamespace(ProjectSpec projectContext, BaseCodeArtifacts artifact);
        string? ExtractNamespace(string content);
        string? ExtractPrimaryClassName(string content);
        string GetProjectName(ProjectSpec projectContext, BaseCodeArtifacts artifact);
        string InferProjectName(string problemStatement);
    }
}