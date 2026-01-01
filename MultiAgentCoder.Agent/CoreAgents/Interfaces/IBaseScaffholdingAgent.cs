using MultiAgentCoder.Domain.Models;
using MultiAgentCoder.Domain.Models.Base;

namespace MultiAgentCoder.Agents.CoreAgents.Interfaces;

public interface IBaseScaffholdingAgent
{
    Task WriteAsync(ProjectSpec projectContext, BaseCodeArtifacts artifact, bool deleteIfExists = true, CancellationToken cancellationToken = default);
    Task WriteAsync(ProjectSpec projectContext, IEnumerable<BaseCodeArtifacts> supportingArtifacts, bool deleteIfExists = false, CancellationToken cancellationToken = default);
}