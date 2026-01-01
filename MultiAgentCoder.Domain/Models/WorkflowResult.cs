using MultiAgentCoder.Domain.Enums;
using MultiAgentCoder.Domain.Models.Base;

namespace MultiAgentCoder.Domain.Models;

/// <summary>
/// Final result of a hybrid agentic workflow execution.
/// </summary>
public sealed class WorkflowResult
{
    /// <summary>
    /// Indicates whether the workflow completed successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Final workflow stage.
    /// </summary>
    public WorkflowStage FinalStage { get; init; }

    /// <summary>
    /// Human-readable summary of what happened.
    /// </summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>
    /// Generated code artifact (if available).
    /// </summary>
    public BaseCodeArtifacts? CodeArtifact { get; init; }

    /// <summary>
    /// Optional diagnostic or failure details.
    /// </summary>
    public string? ErrorDetails { get; init; }

    /* ---------- Factory Methods ---------- */

    public static WorkflowResult SuccessResult(
        WorkflowStage finalStage,
        string summary,
        BaseCodeArtifacts artifact)
    {
        return new WorkflowResult
        {
            Success = true,
            FinalStage = finalStage,
            Summary = summary,
            CodeArtifact = artifact
        };
    }

    public static WorkflowResult FailureResult(
        WorkflowStage finalStage,
        string summary,
        string? errorDetails = null)
    {
        return new WorkflowResult
        {
            Success = false,
            FinalStage = finalStage,
            Summary = summary,
            ErrorDetails = errorDetails
        };
    }
}

