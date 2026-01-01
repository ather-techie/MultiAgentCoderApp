using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAgentCoder.Domain.Models;

public sealed class TestOptions
{
    public bool GenerateUnitTests { get; set; } = true;
    public string TestFramework { get; set; } = "xUnit";
    public bool RunTestsAfterGeneration { get; set; } = true;
}

