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
    private readonly ITestReviewerAgent _reviewerAgent;
    private readonly ITestScaffoldingAgent _testScaffoldingAgent;
    private readonly ITestRunnerAgent _testRunnerAgent;
    private readonly ILogger<QaAgent> _logger;


    public QaAgent(
        IBuildAgent buildAgent,
        IFileService fileService,
        ITestWriterAgent testWriterAgent,
        ITestReviewerAgent reviewerAgent,
        ITestScaffoldingAgent testScaffoldingAgent,
        ITestRunnerAgent testRunnerAgent,
        ILogger<QaAgent> logger)
    {
        _buildAgent = buildAgent;
        _fileService = fileService;
        _testWriterAgent = testWriterAgent;
        _reviewerAgent = reviewerAgent;
        _testScaffoldingAgent = testScaffoldingAgent;
        _testRunnerAgent = testRunnerAgent;
        _logger = logger;
    }

    public async Task<WorkflowResult> ExecuteAsync(
          WorkflowContext context,
         ProjectSpec project,
         CancellationToken cancellationToken = default)
    {
        //project.CodeRootWorkingDirectory = _fileService.GetRootDirectory();

        //var context = new WorkflowContext(project.ProblemStatement);
        context.ProblemStatement = project.ProblemStatement;
        string? feedback = null;
        var reviewResult = new ReviewResult();
        context.CodeArtifact = context.CodeArtifact ?? new CodeArtifact()
        {
            CodeType = CodeType.SourceCode                        
        };

        if (string.IsNullOrEmpty(context.CodeArtifact.Content))
        {
            context.CodeArtifact.Content = _fileService.LoadFile(project, context.CodeArtifact);
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        // Generate UT and review
        for (var i = 0; i < 3; i++)
        {
            // Step 7 started : Generate Unit testing
            _logger.LogInformation($"Step 7 started {(i >= 1 ? "again" : string.Empty)}");
            context.UnitTestArtifact = await _testWriterAgent.GenerateTestsAsync(project, context.CodeArtifact,feedback);

        if (context.UnitTestArtifact is null)
        {
            return WorkflowResult.FailureResult(
                context.CurrentStage, "Unit tests code generation process doesnt work"
            );
        }

        _logger.LogInformation("Generated Unit tests:");
        _logger.LogInformation("--------------------------------------------------");
        _logger.LogInformation(context.UnitTestArtifact.Content);

            // Step 8 started : Review Unit tests
            var reviewResultJson = await _reviewerAgent.ReviewAsync(context.CodeArtifact.Content,context.UnitTestArtifact.Content);

            if (reviewResultJson is null)
            {
                return WorkflowResult.FailureResult(
                    context.CurrentStage, "Review process doesnt work"
                );
            }

            if (!string.IsNullOrWhiteSpace(reviewResultJson))
            {
                _logger.LogInformation("Reviewer Feedback:");
                _logger.LogInformation("--------------------------------------------------");
                _logger.LogInformation(reviewResultJson);
            }

            reviewResult = JsonSerializer.Deserialize<ReviewResult>(reviewResultJson);

            feedback = string.Join(",", (reviewResult?.ReviewComments?.ToArray()) ?? System.Array.Empty<string>());
            context.UnitTestArtifact.Feedbacks.Add(feedback);
            context.AdvanceTo(WorkflowStage.CodeReviewed);

            if (reviewResult?.IsApproved ?? false)
            {
                break;
            }
        }

        if (reviewResult?.IsCritical ?? false)
        {
            return WorkflowResult.FailureResult(
                    context.CurrentStage, "Critical Feedback found in review process"
                );
        }

        // Step 8 started : Unit testing
        _logger.LogInformation("Step 8 started");

        await _testScaffoldingAgent.WriteAsync(project, context.UnitTestArtifact,false);
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

        // Run Unit test and fix any failures
        for (int i = 0; i < 3; i++)
        {
            // Step 9 started : Run Unit tests
            _logger.LogInformation("Step 12 started");
            var testRunResult = await _testRunnerAgent.RunAsync(project, context.UnitTestArtifact);

            if (testRunResult != null)
            {
                _logger.LogInformation("Test Result:");
                _logger.LogInformation("--------------------------------------------------");
                _logger.LogInformation(JsonSerializer.Serialize(testRunResult));
                _logger.LogInformation("--------------------------------------------------");

                if (testRunResult.IsSuccess)
                {
                    context.AdvanceTo(WorkflowStage.TestsPassed);
                    break;
                }
            }

            if (testRunResult != null && !testRunResult.IsSuccess)
            {
                context.AdvanceTo(WorkflowStage.TestsFailed);

                context.CodeArtifact.Feedbacks.AddRange(testRunResult.Errors);
                feedback = string.Join(Environment.NewLine, testRunResult.Errors);

                _logger.LogInformation($"Step 7 started {(i >= 1 ? "again" : string.Empty)}");
                context.UnitTestArtifact = await _testWriterAgent.GenerateTestsAsync(project, context.CodeArtifact, feedback);

                if (context.UnitTestArtifact is null)
                {
                    return WorkflowResult.FailureResult(
                        context.CurrentStage, "Unit tests code generation process doesnt work"
                    );
                }

                _logger.LogInformation("Generated Unit tests:");
                _logger.LogInformation("--------------------------------------------------");
                _logger.LogInformation(context.UnitTestArtifact.Content);
            }
        }

        if (context.CurrentStage == WorkflowStage.TestsFailed)
        {
            return WorkflowResult.FailureResult(
                context.CurrentStage, "Test Failed"
            );
        }

       

        return WorkflowResult.SuccessResult(
            context.CurrentStage, "QA process completed successfully", context.UnitTestArtifact
        );
    }
}        
    
