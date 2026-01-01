using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAgentCoder.Domain.Models;

public sealed class GuardrailOptions
{
    public bool EnforceBuildableCode { get; set; } = true;
    public bool EnforceTestCoverage { get; set; } = false;
    public bool DisallowDynamicCode { get; set; } = true;
    public bool EnforceNamingConventions { get; set; } = true;

    /// <summary>
    /// Max retries allowed for fix loops.
    /// </summary>
    public int MaxAutoFixAttempts { get; set; } = 2;
}

