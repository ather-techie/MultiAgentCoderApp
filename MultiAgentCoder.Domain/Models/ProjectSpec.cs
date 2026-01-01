using MultiAgentCoder.Domain.Enums;

namespace MultiAgentCoder.Domain.Models;

/// <summary>
/// Describes WHAT needs to be built, HOW it should be built,
/// and WHICH agents should participate.
/// This is the contract shared between CLI, Orchestrator and Agents.
/// </summary>
public sealed class ProjectSpec
{
    // -------------------------------
    // 1. High-level intent
    // -------------------------------

    /// <summary>
    /// Natural language problem statement or requirement.
    /// </summary>
    public string ProblemStatement { get; set; } = string.Empty;

    /// <summary>
    /// Optional short description (used in logs, folders, summaries).
    /// </summary>
    public string? Description { get; set; }

    // -------------------------------
    // 2. Project configuration
    // -------------------------------

    public ProjectDescriptor Descriptor { get; set; } = new();

    /// <summary>
    /// Where generated code should be written.
    /// </summary>
    public string RootWorkingDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Type of project being generated.
    /// </summary>
    public ProjectType ProjectType { get; set; } = ProjectType.Console;

    // -------------------------------
    // 3. Execution options
    // -------------------------------

    /// <summary>
    /// Defines which agents should run.
    /// Enables independent or orchestrated execution.
    /// </summary>
    public AgentExecutionOptions Execution { get; set; } = new();

    /// <summary>
    /// Enables strict checks and safety rules.
    /// </summary>
    public GuardrailOptions Guardrails { get; set; } = new();

    // -------------------------------
    // 4. Testing & build options
    // -------------------------------

    public TestOptions Tests { get; set; } = new();

    public BuildOptions Build { get; set; } = new();

    // -------------------------------
    // 5. Advanced / extensibility
    // -------------------------------

    /// <summary>
    /// Arbitrary metadata for experiments, research, or plugins.
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; }
        = new Dictionary<string, string>();
}
