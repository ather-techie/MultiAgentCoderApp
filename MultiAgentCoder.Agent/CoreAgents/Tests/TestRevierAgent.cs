using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MultiAgentCoder.Agents.CoreAgents.Interfaces;
using MultiAgentCoder.Agents.Services.Interfaces;


namespace MultiAgentCoder.Agents.CoreAgents.Tests;

public class TestReviewerAgent : ITestReviewerAgent
{
    private readonly Kernel _kernel;
    private readonly KernelFunction _reviewFunction;
    private readonly IAIOutputCleanerService _cleanerService;

    public TestReviewerAgent(Kernel kernel
        ,IAIOutputCleanerService cleanerService)
    {
        _cleanerService = cleanerService;
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));

        // Load prompt template for review
        _reviewFunction = KernelFunctionFactory.CreateFromPrompt(
            promptTemplate: LoadPrompt("UnitTestReviewer.txt"),
            functionName: "ReviewCSharpUnitTestCode",
            description: "Reviews C# unit test code and generates feedback"
        );
    }

    public async Task<string> ReviewAsync(string code, string utcode, CancellationToken ct = default)
    {
        var settings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = 1500,
            Temperature = 0.2,
            TopP = 0.9
        };

        var arguments = new KernelArguments(settings)
        {
            ["code"] = code,
            ["test"] = utcode
        };

        var result = await _kernel.InvokeAsync(_reviewFunction, arguments, ct);
        var feedback = result.GetValue<string>()?.Trim();

        var clearFeedback = _cleanerService.RemoveMarkdownJsonFence(feedback);

        return clearFeedback ?? "No feedback generated";
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
