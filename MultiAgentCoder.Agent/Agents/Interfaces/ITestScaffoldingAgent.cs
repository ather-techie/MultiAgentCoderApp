using MultiAgentCoder.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAgentCoder.Agents.Agents.Interfaces
{
    public interface ITestScaffoldingAgent : IBaseScaffholdingAgent
    {
        Task<UnitTestCodeArtifacts> EnsureBuildable(ProjectContext projectContext, UnitTestCodeArtifacts artifact);
    }
}
