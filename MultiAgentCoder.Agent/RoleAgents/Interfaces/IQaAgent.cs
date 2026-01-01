using MultiAgentCoder.Console.Orchestration;
using MultiAgentCoder.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAgentCoder.Agents.RoleAgents.Interfaces;

public interface IQaAgent
{
    Task<WorkflowResult> ExecuteAsync(WorkflowContext context, ProjectSpec project, CancellationToken cancellationToken = default);
}
