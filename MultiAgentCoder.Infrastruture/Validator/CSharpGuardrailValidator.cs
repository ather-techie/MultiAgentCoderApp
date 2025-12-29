using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MultiAgentCoder.Domain.Models.Guardrai;

namespace MultiAgentCoder.Infrastructure.Validation;

public static class CSharpGuardrailValidator
{
    public static GuardrailTestResult ValidateUnitTest(
        string code,
        string expectedFramework = "xunit")
    {
        // 1. Syntax validation
        var tree = CSharpSyntaxTree.ParseText(code);
        var errors = tree.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        if (errors.Any())
        {
            return GuardrailTestResult.Fail(
                "C# syntax errors detected:\n" +
                string.Join("\n", errors.Select(e => e.ToString()))
            );
        }

        // 2. Framework enforcement
        //if (expectedFramework == "xunit" &&
        //    code.Contains("Microsoft.VisualStudio.TestTools"))
        //{
        //    return GuardrailTestResult.Fail(
        //        "Use xUnit only. MSTest is not allowed."
        //    );
        //}

        // 3. Truncation check (basic but effective)
        if (code.Count(c => c == '{') != code.Count(c => c == '}'))
        {
            return GuardrailTestResult.Fail(
                "Code appears truncated (unbalanced braces)."
            );
        }

        return GuardrailTestResult.Success();
    }
}
