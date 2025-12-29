namespace MultiAgentCoder.Domain.Models.Guardrai;

public sealed class GuardrailTestResult
{
    public bool IsValid { get; init; }
    public string Feedback { get; init; } = string.Empty;

    public static GuardrailTestResult Success()
        => new() { IsValid = true };

    public static GuardrailTestResult Fail(string feedback)
        => new() { IsValid = false, Feedback = feedback };
}
