using MultiAgentCoder.Domain.Models;
using MultiAgentCoder.Domain.Models.Base;

namespace MultiAgentCoder.Agents.Services.Interfaces
{
    public interface IFileService
    {
        bool CreateDirectory(string path);
        Task DeleteAsync(string workingDirectory, CancellationToken cancellationToken = default);
        (bool Exists, string TestDirPath) DirectoryExists(ProjectContext projectContext, BaseCodeArtifacts artifacts);
        string GetProjectWorkingDirectory(ProjectContext projectContext, BaseCodeArtifacts artifact);
        string GetRootDirectory(string? projectName = null);
        Task<bool> WriteAsync(string rootWorkingDirectory, string projectDirectoryPath, string fileName, string content, CancellationToken cancellationToken = default);
    }
}