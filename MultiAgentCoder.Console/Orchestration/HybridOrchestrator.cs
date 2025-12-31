using Google.Rpc;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using MultiAgentCoder.Agents.Agents.Interfaces;
using MultiAgentCoder.Agents.Services.Interfaces;
using MultiAgentCoder.Console.Orchestration.Interfaces;
using MultiAgentCoder.Domain.Enums;
using MultiAgentCoder.Domain.Models;
using MultiAgentCoder.Domain.Models.Results;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

namespace MultiAgentCoder.Console.Orchestration;

public sealed class HybridOrchestrator : IHybridOrchestrator
{
    private readonly ICoderWritterAgent _coderAgent;
    private readonly IReviewerAgent _reviewerAgent;
    private readonly IBuildAgent _buildAgent;
    private readonly IFileService _fileService;
    private readonly IProjectScaffoldingAgent _projectScaffoldingAgent;
    private readonly ITestWriterAgent _testWriterAgent;
    private readonly ITestScaffoldingAgent _testScaffoldingAgent;
    private readonly ITestRunnerAgent _testRunnerAgent;
    private readonly ILogger _logger;

    public HybridOrchestrator(ICoderWritterAgent coderAgent
        , IReviewerAgent reviewerAgent
        ,IBuildAgent buildAgent
        , IFileService fileService
        , IProjectScaffoldingAgent projectScaffoldingAgent
        , ITestWriterAgent testWriterAgent
        , ITestRunnerAgent testRunnerAgent
        , ITestScaffoldingAgent testScaffoldingAgent
        , ILogger<HybridOrchestrator> logger)
    {
        _coderAgent = coderAgent;
        _reviewerAgent = reviewerAgent;
        _buildAgent = buildAgent;
        _fileService = fileService;
        _projectScaffoldingAgent = projectScaffoldingAgent;
        _testWriterAgent = testWriterAgent;
        _testScaffoldingAgent = testScaffoldingAgent;
        _testRunnerAgent = testRunnerAgent;
        _logger = logger;
    }

    public async Task<WorkflowResult> ExecuteAsync(
        string problemStatement,
        CancellationToken cancellationToken = default)
    {
        var projectContext = new ProjectContext
        {
            Descriptor = new ProjectDescriptor
            {
                Name = "GeneratedProject",
                Language = "C#",
                TargetFramework = "net8.0"
            },
            RootWorkingDirectory = _fileService.GetRootDirectory()
        };

        var context = new WorkflowContext(problemStatement);
        string? feedback = null;
        var reviewResult = new ReviewResult();

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        for (var i = 0; i < 2; i++)
        {

            // STEP 1: Generate code        
            _logger.LogInformation($"Step 1 started {(i >= 1 ? "again" : string.Empty)}");

            context.CodeArtifact = await _coderAgent.GenerateCodeAsync(
                projectContext,
                context.CodeArtifact ?? new CodeArtifact() { CodeType = CodeType.SourceCode },
                problemStatement,
                feedback,
                cancellationToken);

            context.AdvanceTo(WorkflowStage.CodeGenerated);
            var codeArtifact = context.CodeArtifact;


            if (context.CodeArtifact is null)
            {
                return WorkflowResult.FailureResult(
                    context.CurrentStage, "Code generation process doesnt work"
                );
            }

            _logger.LogInformation("Generated Code:");
            _logger.LogInformation("--------------------------------------------------");
            _logger.LogInformation(codeArtifact.Content);


            // STEP 2: Review
            _logger.LogInformation("Step 2 started");

            var reviewResultJson = await _reviewerAgent.ReviewAsync(codeArtifact.Content);

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
            codeArtifact.Feedbacks.Add(feedback);
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

        // STEP 3: Save the generated file locally
        _logger.LogInformation("Step 3 started");
        //await _fileAgent.WriteAsync(projectContext, context.CodeArtifact);
        await _projectScaffoldingAgent.WriteAsync(projectContext,context.CodeArtifact);
        context.CodeArtifact.WorkingDirectory = _fileService.GetProjectWorkingDirectory(projectContext,context.CodeArtifact);
        _logger.LogInformation($"Code saved to: {context.CodeArtifact.WorkingDirectory}");

        // STEP 3: Setup Project Structure (Future)
        // This step would involve setting up the project structure,
        // dependencies, and configuration files as needed.
        // In future, if we already have project then we can skip this step.
        _logger.LogInformation("Step 4 started");
        var csProjCodeArtifacts = await _projectScaffoldingAgent.EnsureBuildable(projectContext,context.CodeArtifact);
        var programCsCodeArtifacts = await _projectScaffoldingAgent.EnsureProgramCsSetup(projectContext,context.CodeArtifact);

        context.SupportingCodeArtifact.AddRange(csProjCodeArtifacts);
        context.SupportingCodeArtifact.AddRange(programCsCodeArtifacts);

        // STEP 3: Save the generated file locally
        _logger.LogInformation("Step 5 started");
        await _projectScaffoldingAgent.WriteAsync(projectContext,context.SupportingCodeArtifact,false);  
        context.AdvanceTo(WorkflowStage.SupportFileCreated);


        // STEP 4: (Future) Build
        _logger.LogInformation("Step 6 started");

        var buildResult = await _buildAgent.BuildAsync(context.CodeArtifact);

        if (buildResult != null)
        {
            _logger.LogInformation("Build Result:");
            _logger.LogInformation("--------------------------------------------------");
            _logger.LogInformation(JsonSerializer.Serialize(buildResult));
            _logger.LogInformation("--------------------------------------------------");

            if (buildResult.IsSuccess)
            {
                context.AdvanceTo(WorkflowStage.BuildSucceeded);
            }
        }
        
        if (buildResult != null && !buildResult.IsSuccess)       
        {
            context.AdvanceTo(WorkflowStage.BuildFailed);

            context.CodeArtifact.Feedbacks.AddRange(buildResult.Errors);
            feedback = string.Join(Environment.NewLine, buildResult.Errors);

            return WorkflowResult.FailureResult(
                context.CurrentStage,"Failed during build"
            );
        }

        // Step 7 started : Unit testing
        _logger.LogInformation("Step 7 started");
       context.UnitTestArtifact = await _testWriterAgent.GenerateTestsAsync(projectContext,context.CodeArtifact);

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

        await _testScaffoldingAgent.WriteAsync(projectContext, context.UnitTestArtifact);
        context.UnitTestArtifact.WorkingDirectory = _fileService.GetProjectWorkingDirectory(projectContext, context.UnitTestArtifact);
        _logger.LogInformation($"Unit tests Code saved to: {context.UnitTestArtifact.WorkingDirectory}");

        // STEP 9: Setup Project Structure (Future)
        // This step would involve setting up the project structure,
        // dependencies, and configuration files as needed.
        // In future, if we already have project then we can skip this step.
        _logger.LogInformation("Step 9 started");
        var csProjUnitTestsCodeArtifacts = await _testScaffoldingAgent.EnsureBuildable(projectContext,context.UnitTestArtifact);

        context.SupportingUnitTestsArtifact.AddRange(csProjUnitTestsCodeArtifacts);

        // STEP 3: Save the generated file locally
        _logger.LogInformation("Step 10 started"); 
        await _testScaffoldingAgent.WriteAsync(projectContext, context.SupportingUnitTestsArtifact, false);
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
        var testRunResult = await _testRunnerAgent.RunAsync(projectContext ,context.UnitTestArtifact);

        if (testRunResult != null)
        {
            _logger.LogInformation("Test Result:");
            _logger.LogInformation("--------------------------------------------------");
            _logger.LogInformation(JsonSerializer.Serialize(testRunResult,options));
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

        // STEP 4: (Future) Unit tests

        return WorkflowResult.SuccessResult(
            context.CurrentStage,
            "Code generated successfully",
            context.CodeArtifact);
    }
}

