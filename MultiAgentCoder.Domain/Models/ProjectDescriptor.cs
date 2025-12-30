using MultiAgentCoder.Domain.Enums;

namespace MultiAgentCoder.Domain.Models;

public sealed class ProjectDescriptor
{
    public string Name { get; init; }
    public string TargetFramework { get; init; } = "net8.0";
    public ProjectType Type { get; init; }

    public string Language { get; init; }
}
