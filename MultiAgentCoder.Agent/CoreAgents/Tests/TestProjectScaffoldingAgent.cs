using MultiAgentCoder.Agents.CoreAgents.Base;
using MultiAgentCoder.Agents.CoreAgents.Interfaces;
using MultiAgentCoder.Agents.Services;
using MultiAgentCoder.Agents.Services.Interfaces;
using MultiAgentCoder.Domain.Enums;
using MultiAgentCoder.Domain.Models;

namespace MultiAgentCoder.Agents.CoreAgents.Tests;

public sealed class TestProjectScaffoldingAgent : BaseScaffholdingAgent ,ITestScaffoldingAgent
{
    public TestProjectScaffoldingAgent(IFileService fileService, IProjectService projectService) : base(fileService,projectService)
    {
    }

    #region "Ensure Buildable Test Project Structure"

    public async Task<UnitTestCodeArtifacts> EnsureBuildable(ProjectSpec projectContext, UnitTestCodeArtifacts artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);

        if (!HasCsproj(projectContext, artifact))
        {
            return CreateDefaultCsproj(projectContext, artifact);
        }

        // TODO: Handle other scenarios like file already exists, test project upgrades, etc.
        return null;
    }

    private bool HasCsproj(ProjectSpec projectContext, UnitTestCodeArtifacts artifact)
    {
        var workingDir = _fileService.GetProjectWorkingDirectory(projectContext, artifact);
        return Directory.GetFiles(workingDir, "*.Tests.csproj", SearchOption.AllDirectories).Any();
    }

    private UnitTestCodeArtifacts CreateDefaultCsproj(ProjectSpec projectContext, UnitTestCodeArtifacts artifact)
    {
        //var safeName = string.IsNullOrWhiteSpace(projectContext.Descriptor.Name)
        //    ? "GeneratedTests"
        //    : projectContext.Descriptor.Name + ".Tests";


        var safeName = _projectService.CreateProjectName(projectContext, artifact);
        var safeReferrenceName = _projectService.CreateProjectName(projectContext, new CodeArtifact() { CodeType = CodeType.SourceCode });

        var csprojContent = $$"""
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

 <ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
  <PackageReference Include="MSTest.TestAdapter" Version="3.1.1" />
  <PackageReference Include="MSTest.TestFramework" Version="3.1.1" />

  <PackageReference Include="FluentAssertions" Version="6.12.0" />
</ItemGroup>


 <ItemGroup>
   <Reference Include="{{safeReferrenceName}}">
     <HintPath>..\{{safeReferrenceName}}\bin\Debug\net8.0\{{safeReferrenceName}}.dll</HintPath>
   </Reference>
 </ItemGroup>

</Project>
""";

        var workingDir = _fileService.GetProjectWorkingDirectory(projectContext, artifact);

        return new UnitTestCodeArtifacts()
        {
            CodeType = CodeType.ProjectFile,
            WorkingDirectory = workingDir,
            SuggestedFileName = $"{safeName}.csproj",
            Content = csprojContent,
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
