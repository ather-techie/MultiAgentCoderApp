using MultiAgentCoder.Agents.CoreAgents.Interfaces;
using MultiAgentCoder.Agents.Services.Interfaces;
using MultiAgentCoder.Domain.Models;
using MultiAgentCoder.Domain.Models.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAgentCoder.Agents.CoreAgents.Base;

public abstract class BaseScaffholdingAgent : IBaseScaffholdingAgent
{
    protected readonly IFileService _fileService;
    protected readonly IProjectService _projectService;

    public BaseScaffholdingAgent(IFileService fileService, IProjectService projectService)
    {
        _fileService = fileService;
        _projectService = projectService;
    }

    public async Task WriteAsync(
    ProjectSpec projectContext,
    BaseCodeArtifacts artifact,
    bool deleteIfExists = true,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(artifact);

        var projectPath = _fileService.GetProjectWorkingDirectory(projectContext, artifact);

        if (deleteIfExists && Directory.Exists(projectPath))
        {
            Directory.Delete(projectPath, recursive: true);
        }

        var projectName = _projectService.CreateProjectName(projectContext, artifact);

        cancellationToken.ThrowIfCancellationRequested();

        await _fileService.WriteAsync(projectContext.RootWorkingDirectory, projectName, artifact.SuggestedFileName, artifact.Content, cancellationToken);
    }

    public async Task WriteAsync(
        ProjectSpec projectContext,
        IEnumerable<BaseCodeArtifacts> supportingArtifacts,
        bool deleteIfExists = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(supportingArtifacts);

        foreach (var artifact in supportingArtifacts)
        {
            await _fileService.WriteAsync(projectContext.RootWorkingDirectory, artifact.WorkingDirectory, artifact.SuggestedFileName, artifact.Content, cancellationToken);
        }
    }

    protected bool HasCsproj(ProjectSpec projectContext, CodeArtifact artifact,string searchPattern, Func<string,bool> predictae)
    {
        var path = _fileService.GetProjectWorkingDirectory(projectContext, artifact);

        return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories)
            .Any(predictae);
    }
}
