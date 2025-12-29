using MultiAgentCoder.Domain.Models;

namespace MultiAgentCoder.Agents.Tools.Interfaces
{
    public interface ICoderTool
    {
        Task<CodeArtifact> GenerateAsync(CodeArtifact artifact, string problem, string feedback);
    }
}