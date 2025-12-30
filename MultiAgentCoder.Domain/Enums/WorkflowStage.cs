namespace MultiAgentCoder.Domain.Enums;

/// <summary>
/// Represents the high-level stages of the agentic workflow.
/// </summary>
public enum WorkflowStage
{
    /// <summary>
    /// Workflow initialized, no agent executed yet.
    /// </summary>
    Initialized = 0,

    /// <summary>
    /// Coder agent has generated initial source code.
    /// </summary>
    CodeGenerated = 1,

    /// <summary>
    /// Code has been reviewed and feedback applied.
    /// </summary>
    CodeReviewed = 2,

    /// <summary>
    /// Code has been reviewed and feedback applied.
    /// </summary>
    SupportFileCreated = 3,

    /// <summary>
    /// dotnet build succeeded.
    /// </summary>
    BuildSucceeded = 4,

    /// <summary>
    /// dotnet build succeeded.
    /// </summary>
    BuildFailed = 5,

    /// <summary>
    /// Unit tests have been generated.
    /// </summary>
    TestsGenerated = 6,

    /// <summary>
    /// All unit tests passed successfully.
    /// </summary>
    TestsPassed = 7,

    /// <summary>
    /// All unit tests passed successfully.
    /// </summary>
    TestsFailed = 8,

    /// <summary>
    /// Workflow failed after retries or unrecoverable error.
    /// </summary>
    Failed = 99
}

