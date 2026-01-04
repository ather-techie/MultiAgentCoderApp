using MultiAgentCoder.Agents.Services.Interfaces;
using MultiAgentCoder.Domain.Enums;
using MultiAgentCoder.Domain.Models;
using MultiAgentCoder.Domain.Models.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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

        //public static string CreateProjectName(ProjectSpec projectContext, BaseCodeArtifacts artifact)
        //{
        //    var type = string.Empty;
        //    switch (artifact.CodeType)
        //    {
        //        case CodeType.SourceCode:
        //            type = ".Code";
        //            break;
        //        case CodeType.UnitTestCode:
        //            type = ".Tests";
        //            break;
        //        default:
        //            break;
        //    }

        //    return $"{projectContext.Descriptor.Name}{type}";

        //}

        public  string? ExtractPrimaryClassName(string content)
        {
            var match = Regex.Match(
                content,
                @"\b(public|internal)\s+(?:sealed\s+|static\s+|partial\s+)?class\s+(?<name>\w+)(?<generics>\s*<[^>{}]+>)?",
                RegexOptions.Multiline);

            if (!match.Success)
                return null;

            var name = match.Groups["name"].Value;
            var generics = match.Groups["generics"].Value;

            return string.IsNullOrWhiteSpace(generics) ? name : name + generics.Trim();
        }

        public  string? ExtractNamespace(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            // Match both file-scoped and block-scoped namespaces
            var match = Regex.Match(
                content,
                @"^\s*namespace\s+(?<name>[\w\.]+)\s*(?:;|\{)",
                RegexOptions.Multiline);

            if (!match.Success)
                return null;

            return match.Groups["name"].Value;
        }


        public static string CreateProjectName(string projectName, CodeType codeType)
        {
            var type = string.Empty;
            switch (codeType)
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

            return $"{projectName}{type}";

        }

        public static string GetRootDirectory(string? projectName = null)
        {
            var safeName = string.IsNullOrWhiteSpace(projectName)
                ? "GeneratedProject"
                : projectName;

            var basePath = AppContext.BaseDirectory;

            var path = Path.Combine(
                basePath,
                "MultiAgentCoder",
                safeName,
                Guid.NewGuid().ToString("N"));

            return path;
        }

        public string GetProjectName(ProjectSpec projectContext, BaseCodeArtifacts artifact)
        {
            return artifact.CodeType == CodeType.SourceCode
                ? projectContext.Descriptor.CodeProjectName
                : projectContext.Descriptor.UnitTestProjectName;
        }

        public string CreateSafeNamespace(ProjectSpec projectContext, BaseCodeArtifacts artifact)
        {
            //var projectName = CreateProjectName(projectContext, artifact);
            var projectName = GetProjectName(projectContext, artifact);

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