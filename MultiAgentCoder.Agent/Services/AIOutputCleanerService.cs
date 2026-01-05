using MultiAgentCoder.Agents.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAgentCoder.Agents.Services;

public class AIOutputCleanerService : IAIOutputCleanerService
{
    public string RemoveMarkdownCSharpFence(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        return content
            .Replace("```csharp", "")
            .Replace("```", "")
            .Trim();
    }

    public string RemoveMarkdownJsonFence(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        input = input.Trim();

        // Remove starting ```json or ```
        if (input.StartsWith("```"))
        {
            var firstNewLine = input.IndexOf('\n');
            if (firstNewLine > -1)
            {
                input = input[(firstNewLine + 1)..];
            }
        }

        // Remove ending ```
        if (input.EndsWith("```"))
        {
            input = input[..input.LastIndexOf("```", StringComparison.Ordinal)].Trim();
        }

        return input.Trim();
    }
}
