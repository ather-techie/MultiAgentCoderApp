using Microsoft.Extensions.Logging;
using MultiAgentCoder.Agents.CoreAgents.Interfaces;
using MultiAgentCoder.Agents.CoreAgents.Ops;
using MultiAgentCoder.Agents.RoleAgents.Interfaces;
using MultiAgentCoder.Agents.Services.Interfaces;
using MultiAgentCoder.Console.Orchestration;
using MultiAgentCoder.Domain.Enums;
using MultiAgentCoder.Domain.Models;
using MultiAgentCoder.Domain.Models.Results;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MultiAgentCoder.Agents.RoleAgents;

public class DevAgent : IDevAgent
{
    private readonly ICodeWriterAgent _coderAgent;
    private readonly ICodeReviewerAgent _reviewerAgent;
    private readonly IBuildAgent _buildAgent;
    private readonly IFileService _fileService;
    private readonly IProjectScaffoldingAgent _projectScaffoldingAgent;

    private readonly ILogger<DevAgent> _logger;

    public DevAgent(ICodeWriterAgent coderAgent, ICodeReviewerAgent reviewerAgent
        , IProjectScaffoldingAgent projectScaffoldingAgent, IBuildAgent buildAgent, IFileService fileService
        , ILogger<DevAgent> logger)
    {
        _coderAgent = coderAgent;
        _reviewerAgent = reviewerAgent;
        _projectScaffoldingAgent = projectScaffoldingAgent;
        _buildAgent = buildAgent;
        _fileService = fileService;

        _logger = logger;
    }

    public async Task<WorkflowResult> ExecuteAsync(
        WorkflowContext context,
       ProjectSpec project,
       CancellationToken cancellationToken = default)
    {
        //var projectContext = new ProjectContext
        //{
        //    Descriptor = new ProjectDescriptor
        //    {
        //        Name = "GeneratedProject",
        //        Language = "C#",
        //        TargetFramework = "net8.0"
        //    },
        //    RootWorkingDirectory = _fileService.GetRootDirectory()
        //};

        project.RootWorkingDirectory = _fileService.GetRootDirectory();

        //var context = new WorkflowContext(project.ProblemStatement);
        context.ProblemStatement = project.ProblemStatement;
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
                project,
                context.CodeArtifact ?? new CodeArtifact() { CodeType = CodeType.SourceCode },
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
        await _projectScaffoldingAgent.WriteAsync(project, context.CodeArtifact);
        context.CodeArtifact.WorkingDirectory = _fileService.GetProjectWorkingDirectory(project, context.CodeArtifact);
        _logger.LogInformation($"Code saved to: {context.CodeArtifact.WorkingDirectory}");

        // STEP 3: Setup Project Structure (Future)
        // This step would involve setting up the project structure,
        // dependencies, and configuration files as needed.
        // In future, if we already have project then we can skip this step.
        _logger.LogInformation("Step 4 started");
        var csProjCodeArtifacts = await _projectScaffoldingAgent.EnsureBuildable(project, context.CodeArtifact);
        var programCsCodeArtifacts = await _projectScaffoldingAgent.EnsureProgramCsSetup(project, context.CodeArtifact);

        context.SupportingCodeArtifact.AddRange(csProjCodeArtifacts);
        context.SupportingCodeArtifact.AddRange(programCsCodeArtifacts);

        // STEP 3: Save the generated file locally
        _logger.LogInformation("Step 5 started");
        await _projectScaffoldingAgent.WriteAsync(project, context.SupportingCodeArtifact, false);
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
                context.CurrentStage, "Failed during build"
            );
        }

        return WorkflowResult.SuccessResult(
            context.CurrentStage,
            "Development workflow completed successfully",
            context.CodeArtifact);
    }


}
