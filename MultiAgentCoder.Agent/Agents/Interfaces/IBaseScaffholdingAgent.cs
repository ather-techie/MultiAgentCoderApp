using MultiAgentCoder.Domain.Models;
using MultiAgentCoder.Domain.Models.Base;

namespace MultiAgentCoder.Agents.Agents.Interfaces
{
    public interface IBaseScaffholdingAgent
    {
        Task WriteAsync(ProjectContext projectContext, BaseCodeArtifacts artifact, bool deleteIfExists = true, CancellationToken cancellationToken = default);
        Task WriteAsync(ProjectContext projectContext, IEnumerable<BaseCodeArtifacts> supportingArtifacts, bool deleteIfExists = false, CancellationToken cancellationToken = default);
    }
}