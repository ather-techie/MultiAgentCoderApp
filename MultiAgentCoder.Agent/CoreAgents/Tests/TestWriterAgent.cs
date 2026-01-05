using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MultiAgentCoder.Agents.CoreAgents.Interfaces;
using MultiAgentCoder.Agents.Services.Interfaces;
using MultiAgentCoder.Domain.Enums;
using MultiAgentCoder.Domain.Models;

namespace MultiAgentCoder.Agents.CoreAgents.Tests;

public sealed class TestWriterAgent : ITestWriterAgent
{
    private readonly Kernel _kernel;
    private readonly KernelFunction _generateTestsFunction;
    private readonly IProjectService _projectService;
    private readonly IAIOutputCleanerService _cleanerService;
    private readonly ILogger<TestWriterAgent> _logger;

    public TestWriterAgent(Kernel kernel,
        IProjectService projectService,
        IAIOutputCleanerService cleanerService,
        ILogger<TestWriterAgent> logger)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _cleanerService = cleanerService;
        _logger = logger;

        _generateTestsFunction =
            KernelFunctionFactory.CreateFromPrompt(
                promptTemplate: LoadPrompt("testwriter.txt"),
                functionName: "GenerateUnitTests",
                description: "Generates C# unit tests for provided production code");
    }

    public async Task<UnitTestCodeArtifacts> GenerateTestsAsync(
        ProjectSpec projectContext,
        CodeArtifact artifact,
        string? feedback = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(artifact);

        var settings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = 2500,
            Temperature = 0.2,
            TopP = 0.9
        };

        var arguments = new KernelArguments(settings)
        {
            ["code"] = artifact.Content,
            ["CodeNamespace"] = _projectService.ExtractNamespace(artifact.Content),
            ["Namespace"] = _projectService.CreateSafeNamespace(projectContext, new UnitTestCodeArtifacts() { CodeType = CodeType.UnitTestCode }),
            ["feedback"] = feedback
        };

        var result = await _kernel.InvokeAsync(
            _generateTestsFunction,
            arguments,
            cancellationToken);

        var testCode = _cleanerService.RemoveMarkdownCSharpFence(result.GetValue<string>());

        if (!IsLikelyComplete(testCode))
        {
            _logger.LogWarning(
                "Generated test code appears to be incomplete for artifact {ArtifactFileName}",
                artifact.SuggestedFileName);
            _logger.LogDebug("Generated test code: {TestCode}", testCode);
            throw new InvalidOperationException("Generated test code appears to be incomplete.");
        }

        //var guardrail = CSharpGuardrailValidator.ValidateUnitTest(testCode);

        //if (guardrail.IsValid)
        //{
        //    return new UnitTestCodeArtifacts
        //    {
        //        CodeType = CodeType.UnitTestCode,
        //        Content = testCode,
        //        SuggestedFileName = $"{Path.GetFileNameWithoutExtension(artifact.SuggestedFileName)}Tests.cs",
        //        Revision = artifact?.Revision is null ? 1 : ++artifact.Revision,
        //        Feedbacks = artifact?.Feedbacks ?? new List<string>(),
        //        LastUpdatedAt = DateTime.UtcNow,
        //        CreatedAt = artifact?.CreatedAt is null ? DateTime.UtcNow : artifact.CreatedAt
        //    };
        //}


        //// Prepare feedback for retry
        //feedback = guardrail.Feedback;
        //_logger.LogWarning(
        //    "Test generation failed guardrails : {Feedback}",
        //     feedback);

        //_logger.LogDebug("Generated test code: {TestCode}", testCode);
        //_logger.LogDebug("Guardrail feedback: {Feedback}", feedback);

        //throw new InvalidOperationException(
        //    "Failed to generate valid unit tests after multiple attempts."
        //);

        return new UnitTestCodeArtifacts
        {
            CodeType = CodeType.UnitTestCode,
            Content = testCode,
            SuggestedFileName = $"{Path.GetFileNameWithoutExtension(artifact.SuggestedFileName)}Tests.cs",
            Revision = artifact?.Revision is null ? 1 : ++artifact.Revision,
            Feedbacks = artifact?.Feedbacks ?? new List<string>(),
            LastUpdatedAt = DateTime.UtcNow,
            CreatedAt = artifact?.CreatedAt is null ? DateTime.UtcNow : artifact.CreatedAt
        };


    }

    

    private bool IsLikelyComplete(string code)
    {
        return code.Count(c => c == '{') == code.Count(c => c == '}');
    }


    private static string LoadPrompt(string fileName)
    {
        var basePath = AppContext.BaseDirectory;
        var path = Path.Combine(basePath, "Prompts", fileName);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Prompt file not found: {path}");

        return File.ReadAllText(path);
    }
}
