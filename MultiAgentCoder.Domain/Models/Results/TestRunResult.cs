namespace MultiAgentCoder.Domain.Models.Results;

public sealed class TestRunResult
{
    public bool IsSuccess { get; init; }
    public string Output { get; init; } = string.Empty;
    public IReadOnlyList<string> Errors { get; init; } = [];
    public TimeSpan Duration { get; init; }
}
