using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAgentCoder.Domain.Models;

public sealed class BuildOptions
{
    public bool BuildAfterGeneration { get; set; } = true;
    public bool TreatWarningsAsErrors { get; set; } = false;
}

