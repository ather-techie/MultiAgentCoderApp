using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAgentCoder.Domain.Enums
{
    public enum CodeType
    {
        Unknown = 0,
        SourceCode = 1,
        UnitTestCode = 2,
        Configuration = 3,
        Documentation = 4,
        Script = 5,
        ProjectFile = 6,
        ProgramEntryPoint = 7
    }
}
