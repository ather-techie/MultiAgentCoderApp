using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MultiAgentCoder.Agents.CoreAgents.Interfaces;
using MultiAgentCoder.Domain.Models;


namespace MultiAgentCoder.Agents.CoreAgents.Dev;

public class CodeReviewerAgent : ICodeReviewerAgent
{
    private readonly Kernel _kernel;
    private readonly KernelFunction _reviewFunction;

    public CodeReviewerAgent(Kernel kernel)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));

        // Load prompt template for review
        _reviewFunction = KernelFunctionFactory.CreateFromPrompt(
            promptTemplate: LoadPrompt("reviewer.txt"),
            functionName: "ReviewCSharpCode",
            description: "Reviews C# code and generates feedback"
        );
    }

    public async Task<string> ReviewAsync(string code, CancellationToken ct = default)
    {
        var settings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = 1500,
            Temperature = 0.2,
            TopP = 0.9
        };

        var arguments = new KernelArguments (settings)
        {
            ["code"] = code
        };

        var result = await _kernel.InvokeAsync(_reviewFunction, arguments, ct);
        var feedback = result.GetValue<string>()?.Trim();

        return feedback ?? "No feedback generated";
    }

    private static string LoadPrompt(string fileName)
    {
        var basePath = AppContext.BaseDirectory;
        var promptPath = Path.Combine(basePath, "Prompts", fileName);

        if (!File.Exists(promptPath))
            throw new FileNotFoundException($"Prompt file not found: {promptPath}");

        return File.ReadAllText(promptPath);
    }
}
