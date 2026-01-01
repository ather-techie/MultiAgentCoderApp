using Microsoft.Extensions.Logging;
using MultiAgentCoder.Agents.CoreAgents.Interfaces;
using MultiAgentCoder.Agents.RoleAgents.Interfaces;
using MultiAgentCoder.Agents.Services.Interfaces;
using MultiAgentCoder.Console.Orchestration;
using MultiAgentCoder.Domain.Enums;
using MultiAgentCoder.Domain.Models;
using MultiAgentCoder.Domain.Models.Results;
using System.Text.Json;

namespace MultiAgentCoder.Agents.RoleAgents;

public class QaAgent : IQaAgent
{

    private readonly IBuildAgent _buildAgent;
    private readonly IFileService _fileService;
    private readonly ITestWriterAgent _testWriterAgent;
    private readonly ITestScaffoldingAgent _testScaffoldingAgent;
    private readonly ITestRunnerAgent _testRunnerAgent;
    private readonly ILogger<QaAgent> _logger;


    public QaAgent(
        IBuildAgent buildAgent,
        IFileService fileService,
        ITestWriterAgent testWriterAgent,
        ITestScaffoldingAgent testScaffoldingAgent,
        ITestRunnerAgent testRunnerAgent,
        ILogger<QaAgent> logger)
    {
        _buildAgent = buildAgent;
        _fileService = fileService;
        _testWriterAgent = testWriterAgent;
        _testScaffoldingAgent = testScaffoldingAgent;
        _testRunnerAgent = testRunnerAgent;
        _logger = logger;
    }

    public async Task<WorkflowResult> ExecuteAsync(
          WorkflowContext context,
         ProjectSpec project,
         CancellationToken cancellationToken = default)
    {
        project.RootWorkingDirectory = _fileService.GetRootDirectory();

        //var context = new WorkflowContext(project.ProblemStatement);
        context.ProblemStatement = project.ProblemStatement;
        string? feedback = null;
        var reviewResult = new ReviewResult();

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        // Step 7 started : Unit testing
        _logger.LogInformation("Step 7 started");
        context.UnitTestArtifact = await _testWriterAgent.GenerateTestsAsync(project, context.CodeArtifact);

        if (context.UnitTestArtifact is null)
        {
            return WorkflowResult.FailureResult(
                context.CurrentStage, "Unit tests code generation process doesnt work"
            );
        }

        _logger.LogInformation("Generated Unit tests:");
        _logger.LogInformation("--------------------------------------------------");
        _logger.LogInformation(context.UnitTestArtifact.Content);

        // Step 8 started : Unit testing
        _logger.LogInformation("Step 8 started");

        await _testScaffoldingAgent.WriteAsync(project, context.UnitTestArtifact);
        context.UnitTestArtifact.WorkingDirectory = _fileService.GetProjectWorkingDirectory(project, context.UnitTestArtifact);
        _logger.LogInformation($"Unit tests Code saved to: {context.UnitTestArtifact.WorkingDirectory}");

        // STEP 9: Setup Project Structure (Future)
        // This step would involve setting up the project structure,
        // dependencies, and configuration files as needed.
        // In future, if we already have project then we can skip this step.
        _logger.LogInformation("Step 9 started");
        var csProjUnitTestsCodeArtifacts = await _testScaffoldingAgent.EnsureBuildable(project, context.UnitTestArtifact);

        context.SupportingUnitTestsArtifact.AddRange(csProjUnitTestsCodeArtifacts);

        // STEP 3: Save the generated file locally
        _logger.LogInformation("Step 10 started");
        await _testScaffoldingAgent.WriteAsync(project, context.SupportingUnitTestsArtifact, false);
        context.AdvanceTo(WorkflowStage.SupportFileCreated);

        // Step 9 : Build tests project
        _logger.LogInformation("Step 11 started");
        var buildTestProj = await _buildAgent.BuildAsync(context.UnitTestArtifact);

        if (buildTestProj != null)
        {
            _logger.LogInformation("Test  Build Result:");
            _logger.LogInformation("--------------------------------------------------");
            _logger.LogInformation(JsonSerializer.Serialize(buildTestProj));
            _logger.LogInformation("--------------------------------------------------");

            if (buildTestProj.IsSuccess)
            {
                context.AdvanceTo(WorkflowStage.BuildSucceeded);
            }
        }

        if (buildTestProj != null && !buildTestProj.IsSuccess)
        {
            context.AdvanceTo(WorkflowStage.BuildFailed);

            context.CodeArtifact.Feedbacks.AddRange(buildTestProj.Errors);
            feedback = string.Join(Environment.NewLine, buildTestProj.Errors);

            return WorkflowResult.FailureResult(
                context.CurrentStage, "Failed during build"
            );
        }

        // Step 9 started : Run Unit tests
        _logger.LogInformation("Step 12 started");
        var testRunResult = await _testRunnerAgent.RunAsync(project, context.UnitTestArtifact);

        if (testRunResult != null)
        {
            _logger.LogInformation("Test Result:");
            _logger.LogInformation("--------------------------------------------------");
            _logger.LogInformation(JsonSerializer.Serialize(testRunResult));
            _logger.LogInformation("--------------------------------------------------");

            if (!testRunResult.IsSuccess)
            {
                context.AdvanceTo(WorkflowStage.TestsPassed);
            }
        }

        if (testRunResult != null && !testRunResult.IsSuccess)
        {
            context.AdvanceTo(WorkflowStage.TestsFailed);

            context.CodeArtifact.Feedbacks.AddRange(testRunResult.Errors);
            feedback = string.Join(Environment.NewLine, testRunResult.Errors);

            return WorkflowResult.FailureResult(
                context.CurrentStage, "Test Failed"
            );
        }

        return WorkflowResult.SuccessResult(
            context.CurrentStage, "QA process completed successfully", context.UnitTestArtifact
        );
    }
}        
    
