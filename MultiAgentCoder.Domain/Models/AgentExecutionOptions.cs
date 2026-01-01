using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAgentCoder.Domain.Models;

public sealed class AgentExecutionOptions
{
    public bool RunCodeAgent { get; set; } = true;
    public bool RunScaffoldingAgent { get; set; } = true;
    public bool RunReviewerAgent { get; set; } = true;
    public bool RunTestAgent { get; set; } = true;
    public bool RunBuildAgent { get; set; } = true;

    /// <summary>
    /// If true, stop execution on first failure.
    /// </summary>
    public bool FailFast { get; set; } = true;
}

