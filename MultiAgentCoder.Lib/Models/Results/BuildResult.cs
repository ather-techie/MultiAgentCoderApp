namespace MultiAgentCoder.Domain.Models.Results;

public sealed class BuildResult
{
    public bool IsSuccess { get; init; }
    public string BuildOutput { get; init; } = string.Empty;
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public TimeSpan Duration { get; init; }
}
