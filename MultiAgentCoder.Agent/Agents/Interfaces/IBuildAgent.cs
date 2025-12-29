using MultiAgentCoder.Domain.Models.Base;
using MultiAgentCoder.Domain.Models.Results;

namespace MultiAgentCoder.Agents.Agents.Interfaces
{
    public interface IBuildAgent
    {
        Task<BuildResult> BuildAsync(BaseCodeArtifacts artifact, string? msBuildArguments = null, CancellationToken cancellationToken = default);
    }
}