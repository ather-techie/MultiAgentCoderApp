using Google.Cloud.Iam.V1;
using Google.Rpc;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MultiAgentCoder.Agents.Agents.Interfaces;
using MultiAgentCoder.Agents.Services.Interfaces;
using MultiAgentCoder.Domain.Enums;
using MultiAgentCoder.Domain.Models;
using System;
using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace MultiAgentCoder.Agents.Agents;

/// <summary>
/// Semantic Kernel based agent responsible for generating and fixing C# code.
/// </summary>
public sealed class CodeWritterrAgent : ICoderWritterAgent
{
    private readonly Kernel _kernel;
    private readonly ILogger<CodeWritterrAgent> _logger;
    private readonly IFileService _fileService;
    private readonly IProjectService _projectService;
    private readonly KernelFunction _generateCodeFunction;

    public CodeWritterrAgent(Kernel kernel,
        IFileService fileService,
        IProjectService projectService,
        ILogger<CodeWritterrAgent> logger)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger;

        _generateCodeFunction = KernelFunctionFactory.CreateFromPrompt(
            promptTemplate: LoadPrompt("coder.txt"),
            functionName: "GenerateCSharpCode",
            description: "Generates or fixes C# code based on problem and feedback"
        );
    }

    /// <summary>
    /// Generates or fixes C# code using Semantic Kernel.
    /// </summary>
    public async Task<CodeArtifact> GenerateCodeAsync(
        ProjectContext projectContext,
        CodeArtifact artifact,
        string problemStatement,
        string? feedback,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(problemStatement))
            throw new ArgumentException("Problem statement is required.", nameof(problemStatement));

        var settings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = 1800,
            Temperature = 0.2,
            TopP = 0.9
        };

        artifact = artifact ?? new CodeArtifact()
        {
            CodeType = CodeType.SourceCode
        };

        var arguments = new KernelArguments (settings)
        {
            ["problem"] = problemStatement,
            ["Namespace"] = _projectService.CreateSafeNamespace(projectContext,artifact),
            ["feedback"] = feedback ?? string.Empty            
        };

        var result = await _kernel.InvokeAsync(
            _generateCodeFunction,
            arguments,
            cancellationToken
        );

        var generatedCode = result.GetValue<string>()?.Trim();

        if (string.IsNullOrWhiteSpace(generatedCode))
            throw new InvalidOperationException("CoderAgent returned empty code.");

        var sanitizeGeneratedCode = StripMarkdownCodeFence(generatedCode);

        return new CodeArtifact()
        {
            CodeType = CodeType.SourceCode,
            Content = sanitizeGeneratedCode,
            Revision = artifact?.Revision is  null ? 1 : ++artifact.Revision,
            Feedbacks = artifact?.Feedbacks ?? new List<string>(),
            LastUpdatedAt = DateTime.UtcNow,
            CreatedAt = artifact?.CreatedAt is null ?  DateTime.UtcNow : artifact.CreatedAt
        };
    }

    /* ---------- Helpers ---------- */

    private static string LoadPrompt(string fileName)
    {
        var basePath = AppContext.BaseDirectory;
        var promptPath = Path.Combine(basePath, "Prompts", fileName);

        if (!File.Exists(promptPath))
            throw new FileNotFoundException($"Prompt file not found: {promptPath}");

        return File.ReadAllText(promptPath);
    }

    //private static string InferProjectName(string problemStatement)
    //{
    //    // Simple deterministic heuristic
    //    var words = problemStatement
    //        .Split(' ', StringSplitOptions.RemoveEmptyEntries);

    //    return words.Length > 0
    //        ? $"{words[0]}Project"
    //        : "GeneratedProject";
    //}

    private static string StripMarkdownCodeFence(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return content;

        content = content.Trim();

        // Remove opening fence ``` or ```csharp
        if (content.StartsWith("```"))
        {
            var firstNewLine = content.IndexOf('\n');
            if (firstNewLine >= 0)
            {
                content = content[(firstNewLine + 1)..];
            }
        }

        // Remove closing fence ```
        if (content.EndsWith("```"))
        {
            content = content[..content.LastIndexOf("```", StringComparison.Ordinal)];
        }

        return content.Trim();
    }
}
