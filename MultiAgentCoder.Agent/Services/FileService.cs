using Google.Cloud.AIPlatform.V1;
using MultiAgentCoder.Agents.Agents.Interfaces;
using MultiAgentCoder.Agents.Services.Interfaces;
using MultiAgentCoder.Domain.Enums;
using MultiAgentCoder.Domain.Models;
using MultiAgentCoder.Domain.Models.Base;

namespace MultiAgentCoder.Agents.Services;

// TOSO : Move some logic into FileService in future.

public sealed class FileService : IFileService
{
    private readonly IProjectService _projectService;

    public FileService(IProjectService projectService)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
    }

    public async Task<bool> WriteAsync(
        string rootWorkingDirectory,
        string projectDirectoryPath,
        string fileName,
        string content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rootWorkingDirectory);
        ArgumentNullException.ThrowIfNull(projectDirectoryPath);
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(content);

        var directoryCreated = CreateDirectory(Path.Combine(rootWorkingDirectory, projectDirectoryPath));

        var fullPath = Path.Combine(rootWorkingDirectory, projectDirectoryPath, fileName);

        await File.WriteAllTextAsync(
            fullPath,
            content,
            cancellationToken);

        return true;
    }

    public Task DeleteAsync(
        string workingDirectory,
        CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(workingDirectory))
        {
            Directory.Delete(workingDirectory, recursive: true);
        }

        return Task.CompletedTask;
    }

    public bool CreateDirectory(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (Directory.Exists(path))
        {
            return false;
        }

        Directory.CreateDirectory(path);

        return true;
    }

    public (bool Exists, string TestDirPath ) DirectoryExists(ProjectContext projectContext,BaseCodeArtifacts artifacts)
    {
        ArgumentNullException.ThrowIfNull(artifacts);

        var projectDir =
           GetProjectWorkingDirectory(projectContext, artifacts);

        return (Directory.Exists(projectDir),projectDir);
    }

    public string GetRootDirectory(string? projectName = null)
    {
        var safeName = string.IsNullOrWhiteSpace(projectName)
            ? "GeneratedProject"
            : projectName;

        var basePath = AppContext.BaseDirectory;

        var path = Path.Combine(
            basePath,
            "MultiAgentCoder",
            safeName,
            Guid.NewGuid().ToString("N"));

        return path;
    }

    public string GetProjectWorkingDirectory(ProjectContext projectContext, BaseCodeArtifacts artifact)
    {
        var projectPath = _projectService.CreateProjectName(projectContext, artifact);

        return Path.Combine(projectContext.RootWorkingDirectory, projectPath);
    }

    //public string CreateProjectName(ProjectContext projectContext, BaseCodeArtifacts artifact)
    //{
    //    var type = string.Empty;
    //    switch (artifact.CodeType)
    //    {
    //        case CodeType.SourceCode:
    //            type = ".Code";
    //            break;
    //        case CodeType.UnitTestCode:
    //            type = ".Tests";
    //            break;
    //        default:
    //            break;
    //    }

    //    return $"{projectContext.Descriptor.Name}{type}";

    //}
}
