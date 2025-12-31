using Google.Cloud.AIPlatform.V1;
using MultiAgentCoder.Agents.Agents.Base;
using MultiAgentCoder.Agents.Agents.Interfaces;
using MultiAgentCoder.Agents.Services.Interfaces;
using MultiAgentCoder.Domain.Enums;
using MultiAgentCoder.Domain.Models;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Linq;

namespace MultiAgentCoder.Agents.Agents.Dev;

public sealed class ProjectScaffoldingAgent : BaseScaffholdingAgent, IProjectScaffoldingAgent
{
    public ProjectScaffoldingAgent(IFileService fileService, IProjectService projectService) : base(fileService,projectService)
    {
    }

    

    #region "Ensure Buildable Project Structure"

    public async Task<CodeArtifact> EnsureBuildable(ProjectContext projectContext, CodeArtifact artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);

        if (!HasCsproj(projectContext, artifact))
        {
            return CreateDefaultCsproj(projectContext, artifact);
        }
        // TODO : Handle other scenarios like file alrrady exists etc.

        return null;
    }

    private bool HasCsproj(ProjectContext projectContext, CodeArtifact artifact)
    {
       return base.HasCsproj(projectContext, artifact,"*.Code.csproj",_ => true);
    }

    private CodeArtifact CreateDefaultCsproj(ProjectContext projectContext, CodeArtifact artifact)
    {
        //var safeName = string.IsNullOrWhiteSpace(projectContext.Descriptor.Name)
        //    ? "GeneratedProject"
        //    : projectContext.Descriptor.Name;

        var safeName = _projectService.CreateProjectName(projectContext, artifact);


        //TODO: Enhance to support different project types, frameworks, etc.
        // For now, we create a simple .NET 8.0 console application project file.
        // will project output type be needed here? Library, Exe, WinExe, etc.
        // Also consider adding LangVersion, Nullable, ImplicitUsings, etc.
        // Refer to the related information for an example csproj structure.

        var csprojContent = """
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
""";

        return new CodeArtifact()
        {
            WorkingDirectory = artifact.WorkingDirectory,
            SuggestedFileName = $"{safeName}.csproj",
            Content = csprojContent,
            CreatedAt = DateTime.UtcNow,
        };
    }

    #endregion

    #region "Ensure Program.cs Setup"
    private bool HasProgramCs(ProjectContext projectContext, CodeArtifact artifact)
    {
        return base.HasCsproj(projectContext, artifact, "*", f => string.Equals(
               Path.GetFileName(f),
               "Program.cs",
               StringComparison.OrdinalIgnoreCase));

        //var path = _fileService.GetProjectWorkingDirectory(projectContext, artifact);
        //return Directory.GetFiles(path, "*", SearchOption.AllDirectories)
        //   .Any(f => string.Equals(
        //       Path.GetFileName(f),
        //       "Program.cs",
        //       StringComparison.OrdinalIgnoreCase));
    }


    private CodeArtifact? CreateProgramCs(ProjectContext projectContext, CodeArtifact artifact)
    {
        var className = ExtractPrimaryClassName(artifact.Content) ?? "MyClass";
        var safeName = _projectService.CreateProjectName(projectContext, artifact);

        if (projectContext.Descriptor.Type != ProjectType.Executable)
        {
            return null;
        }

        // Build instantiation code that handles both generic and non-generic classes.
        string instantiationLine;
        if (string.IsNullOrWhiteSpace(className))
        {
            instantiationLine = "// TODO: Instantiate generated class";
        }
        else if (className.Contains('<'))
        {
            var idx = className.IndexOf('<');
            var baseName = className.Substring(0, idx);
            var genericsInner = className.Substring(idx).Trim(); // e.g. "<T, U>"
            var inner = genericsInner.Trim('<', '>').Trim();
            var paramCount = string.IsNullOrWhiteSpace(inner) ? 0 : inner.Split(',').Length;
            var genericArgs = paramCount == 0 ? "object" : string.Join(", ", Enumerable.Repeat("object", paramCount));
            instantiationLine = $"var app = new {baseName}<{genericArgs}>();";
        }
        else
        {
            instantiationLine = $"var app = new {className}();";
        }


        var programCsContent = $$"""
using System;
using {{safeName}};

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Application started.");

        {{instantiationLine}}
    }
}
""";

        return new CodeArtifact()
        {
            WorkingDirectory = artifact.WorkingDirectory,
            SuggestedFileName = $"Program.cs",
            Content = programCsContent,
            CreatedAt = DateTime.UtcNow,
        };
    }

    private static string? ExtractPrimaryClassName(string content)
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

    public async Task<CodeArtifact> EnsureProgramCsSetup(ProjectContext projectContext, CodeArtifact artifact)
    {
        // TODO : Project type will decide do we need this file or not
        // For now, we assume console application which needs Program.cs
        // Later we can enhance this to handle web apps, libraries, etc.
        ArgumentNullException.ThrowIfNull(artifact);

        if (!HasProgramCs(projectContext, artifact))
        {
            return CreateProgramCs(projectContext, artifact);
        }
        // TODO : Handle other scenarios like file alrrady exists etc.

        return null;
    }
    #endregion 

}
