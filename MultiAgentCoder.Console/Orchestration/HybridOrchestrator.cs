using Google.Rpc;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using MultiAgentCoder.Agents.CoreAgents.Interfaces;
using MultiAgentCoder.Agents.RoleAgents.Interfaces;
using MultiAgentCoder.Agents.Services.Interfaces;
using MultiAgentCoder.Console.Orchestration.Interfaces;
using MultiAgentCoder.Domain.Enums;
using MultiAgentCoder.Domain.Models;
using MultiAgentCoder.Domain.Models.Results;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

namespace MultiAgentCoder.Console.Orchestration;

public sealed class HybridOrchestrator : IHybridOrchestrator
{
    private readonly IDevAgent _devAgent;
    private readonly IQaAgent _qaAgent;
    private readonly ILogger _logger;

    public HybridOrchestrator(
        IDevAgent devAgent
        , IQaAgent qaAgent
        , ILogger<HybridOrchestrator> logger)
    {
        _devAgent = devAgent;
        _qaAgent = qaAgent;
        _logger = logger;
    }

    public async Task<WorkflowResult> ExecuteAsync(
        ProjectSpec project,         
        CancellationToken cancellationToken = default)
    {
        var context = new WorkflowContext(project.ProblemStatement);
        var reviewResult = new ReviewResult();
        var codeResult = await _devAgent.ExecuteAsync(context, project, cancellationToken);

        if (!codeResult.Success)
        {
            return codeResult;            
        }

        var testResult = await _qaAgent.ExecuteAsync(context, project, cancellationToken);

        if (!testResult.Success)
        {
            return testResult;
        }

        return WorkflowResult.SuccessResult(
            context.CurrentStage,
            "Code generated successfully",
            context.CodeArtifact);
    }
}

