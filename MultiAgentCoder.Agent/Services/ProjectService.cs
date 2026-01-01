using MultiAgentCoder.Agents.Services.Interfaces;
using MultiAgentCoder.Domain.Enums;
using MultiAgentCoder.Domain.Models;
using MultiAgentCoder.Domain.Models.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAgentCoder.Agents.Services
{
    public class ProjectService : IProjectService
    {
        public string InferProjectName(string problemStatement)
        {
            // Simple deterministic heuristic
            var words = problemStatement
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return words.Length > 0
                ? $"{words[0]}Project"
                : "GeneratedProject";
        }

        public string CreateProjectName(ProjectSpec projectContext, BaseCodeArtifacts artifact)
        {
            var type = string.Empty;
            switch (artifact.CodeType)
            {
                case CodeType.SourceCode:
                    type = ".Code";
                    break;
                case CodeType.UnitTestCode:
                    type = ".Tests";
                    break;
                default:
                    break;
            }

            return $"{projectContext.Descriptor.Name}{type}";

        }

        public string CreateSafeNamespace(ProjectSpec projectContext, BaseCodeArtifacts artifact)
        {
            var projectName = CreateProjectName(projectContext, artifact);
            // Replace invalid namespace characters
            var builder = new StringBuilder();
            foreach (var ch in projectName)
            {
                if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '.')
                {
                    builder.Append(ch);
                }
                else
                {
                    builder.Append('_');
                }
            }
            return builder.ToString();
        }
    }
}