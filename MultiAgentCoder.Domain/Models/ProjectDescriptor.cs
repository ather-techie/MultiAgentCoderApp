using MultiAgentCoder.Domain.Enums;

namespace MultiAgentCoder.Domain.Models;

public sealed class ProjectDescriptor
{
    public string Name { get; set; }
    public string TargetFramework { get; set; } = "net8.0";
    public ProjectType Type { get; set; }

    public string Language { get; set; }
    public string? RootNamespace { get; set; }
}
