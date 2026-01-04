using MultiAgentCoder.Domain.Models;
using MultiAgentCoder.Domain.Models.Base;

namespace MultiAgentCoder.Agents.Services.Interfaces
{
    public interface IFileService
    {
        bool CreateDirectory(string path);
        Task DeleteAsync(string workingDirectory, CancellationToken cancellationToken = default);
        (bool Exists, string DirPath) DirectoryExists(ProjectSpec projectContext, BaseCodeArtifacts artifacts);
        string GetProjectWorkingDirectory(ProjectSpec projectContext, BaseCodeArtifacts artifact);
        string LoadFile(ProjectSpec project, BaseCodeArtifacts artifacts);
        Task<bool> WriteAsync(string rootWorkingDirectory, string projectDirectoryPath, string fileName, string content, CancellationToken cancellationToken = default);
    }
}