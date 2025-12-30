
using MultiAgentCoder.Domain.Enums;
using MultiAgentCoder.Domain.Models;

namespace MultiAgentCoder.Console.Orchestration;

/// <summary>
/// Holds mutable state for a single workflow execution.
/// </summary>
public sealed class WorkflowContext
{
    private readonly Dictionary<WorkflowStage, int> _retryCounts = new();

    public Guid WorkflowId { get; } = Guid.NewGuid();

    public string ProblemStatement { get; }

    public WorkflowStage CurrentStage { get; private set; }

    public ProjectContext Project { get; init; } = default!;

    public CodeArtifact? CodeArtifact { get; set; }

    public List<CodeArtifact?> SupportingCodeArtifact { get; set; } = new(); // e.g., scaffolding, dependencies, csproj, program.cs etc.

    public UnitTestCodeArtifacts? UnitTestArtifact { get; set; }

    public List<UnitTestCodeArtifacts?> SupportingUnitTestsArtifact { get; set; } = new(); // e.g., scaffolding, dependencies, csproj, program.cs etc.


    public string? LastError { get; private set; }

    public WorkflowContext(string problemStatement)
    {
        ProblemStatement = problemStatement
            ?? throw new ArgumentNullException(nameof(problemStatement));

        CurrentStage = WorkflowStage.Initialized;
    }

    /* ---------- State Transitions ---------- */

    public void AdvanceTo(WorkflowStage nextStage)
    {
        CurrentStage = nextStage;
    }

    public void MarkFailed(string error)
    {
        LastError = error;
        CurrentStage = WorkflowStage.Failed;
    }

    /* ---------- Retry Handling ---------- */

    public int IncrementRetry(WorkflowStage stage)
    {
        if (!_retryCounts.ContainsKey(stage))
        {
            _retryCounts[stage] = 0;
        }

        _retryCounts[stage]++;
        return _retryCounts[stage];
    }

    public int GetRetryCount(WorkflowStage stage)
    {
        return _retryCounts.TryGetValue(stage, out var count)
            ? count
            : 0;
    }

    public bool CanRetry(WorkflowStage stage, int maxRetries)
    {
        return GetRetryCount(stage) < maxRetries;
    }

    /* ---------- Diagnostics ---------- */

    public override string ToString()
    {
        return $"""
        Stage      : {CurrentStage}
        Retries    : {GetRetryCount(CurrentStage)}
        Has Code   : {CodeArtifact != null}
        Last Error : {LastError ?? "None"}
        """;
    }
}

