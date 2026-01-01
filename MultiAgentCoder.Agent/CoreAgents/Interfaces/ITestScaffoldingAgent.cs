using MultiAgentCoder.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAgentCoder.Agents.CoreAgents.Interfaces;

public interface ITestScaffoldingAgent : IBaseScaffholdingAgent
{
    Task<UnitTestCodeArtifacts> EnsureBuildable(ProjectSpec projectContext, UnitTestCodeArtifacts artifact);
}
