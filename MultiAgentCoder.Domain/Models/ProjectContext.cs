using MultiAgentCoder.Domain.Models.Results;

namespace MultiAgentCoder.Domain.Models;

public sealed class ProjectContext
{
    public ProjectDescriptor Descriptor { get; init; }
    public string RootWorkingDirectory { get; set; }
    public BuildResult? LastBuildResult { get; set; }
    public BuildResult? LastTestResult { get; set; }
}
