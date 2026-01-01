using MultiAgentCoder.Domain.Enums;
using MultiAgentCoder.Domain.Models;

namespace MultiAgentCoder.Console.Extensions;

public static class ProjectSpecExtensions
{
    public static ProjectSpec Fill(this ProjectSpec spec, string[] args)
    {
        ArgumentNullException.ThrowIfNull(spec);

        var dict = ParseArgs(args);

        // -------------------------
        // Core problem
        // -------------------------
        spec.ProblemStatement  = Get(dict, "problem", spec.ProblemStatement);
        spec.Description = Get(dict, "description", spec.Description);

        // -------------------------
        // Project
        // -------------------------
        spec.Descriptor ??= new ProjectDescriptor();

        spec.Descriptor.Name = Get(dict, "name", spec.Descriptor.Name ?? "GeneratedProject");
        spec.Descriptor.Language = Get(dict, "language", spec.Descriptor.Language ?? "C#");
        spec.Descriptor.TargetFramework = Get(dict, "framework", spec.Descriptor.TargetFramework ?? "net8.0");
        spec.Descriptor.RootNamespace = Get(dict, "namespace", spec.Descriptor.RootNamespace ?? spec.Descriptor.Name);

        spec.RootWorkingDirectory = Get(dict, "out", spec.RootWorkingDirectory ?? "output");

        spec.ProjectType = Enum.TryParse<ProjectType>(
            Get(dict, "type", spec.ProjectType.ToString()),
            true,
            out var type)
            ? type
            : ProjectType.Console;

        // -------------------------
        // Execution
        // -------------------------
        spec.Execution ??= new AgentExecutionOptions();

        spec.Execution.RunCodeAgent = GetBool(dict, "code", spec.Execution.RunCodeAgent, defaultValue: true);
        spec.Execution.RunScaffoldingAgent = GetBool(dict, "scaffold", spec.Execution.RunScaffoldingAgent, true);
        spec.Execution.RunReviewerAgent = GetBool(dict, "review", spec.Execution.RunReviewerAgent, true);
        spec.Execution.RunTestAgent = GetBool(dict, "tests", spec.Execution.RunTestAgent, true);
        spec.Execution.RunBuildAgent = GetBool(dict, "build", spec.Execution.RunBuildAgent, true);
        spec.Execution.FailFast = GetBool(dict, "failfast", spec.Execution.FailFast, true);

        // -------------------------
        // Guardrails
        // -------------------------
        spec.Guardrails ??= new GuardrailOptions();

        spec.Guardrails.EnforceBuildableCode = true;
        spec.Guardrails.EnforceNamingConventions = true;
        spec.Guardrails.MaxAutoFixAttempts =
            GetInt(dict, "maxfix", spec.Guardrails.MaxAutoFixAttempts, 2);

        // -------------------------
        // Tests
        // -------------------------
        spec.Tests ??= new TestOptions();

        spec.Tests.GenerateUnitTests = spec.Execution.RunTestAgent;
        spec.Tests.RunTestsAfterGeneration = spec.Execution.RunTestAgent;

        // -------------------------
        // Build
        // -------------------------
        spec.Build ??= new BuildOptions();

        spec.Build.BuildAfterGeneration = spec.Execution.RunBuildAgent;

        return spec;
    }

    // =========================
    // Helpers
    // =========================

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var arg in args)
        {
            if (!arg.StartsWith("--")) continue;

            var parts = arg[2..].Split('=', 2);
            var key = parts[0];
            var value = parts.Length > 1 ? parts[1] : "true";

            dict[key] = value;
        }

        return dict;
    }

    private static string? Get(
        Dictionary<string, string> dict,
        string key,
        string? current)
        => dict.TryGetValue(key, out var value) ? value : current;

    private static bool GetBool(
        Dictionary<string, string> dict,
        string key,
        bool current,
        bool defaultValue)
    {
        if (dict.TryGetValue($"no-{key}", out _))
            return false;

        if (dict.TryGetValue(key, out var value) &&
            bool.TryParse(value, out var result))
            return result;

        return current || defaultValue;
    }

    private static int GetInt(
        Dictionary<string, string> dict,
        string key,
        int current,
        int defaultValue)
    {
        if (dict.TryGetValue(key, out var value) &&
            int.TryParse(value, out var result))
            return result;

        return current > 0 ? current : defaultValue;
    }
}
