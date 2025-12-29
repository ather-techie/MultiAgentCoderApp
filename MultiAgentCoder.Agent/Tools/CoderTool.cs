//using AutoGen.Core;
//using MultiAgentCoder.Agents.Agents;
//using MultiAgentCoder.Agents.Tools.Interfaces;
//using MultiAgentCoder.Domain.Models;

//namespace MultiAgentCoder.Agents.Tools;

///// <summary>
///// Created for the CoderAgent to generate or fix C# code based on problem descriptions and feedback.
///// Use with AutoGen framework.
///// </summary>
//public class CoderTool : ICoderTool
//{
//    private readonly CoderAgent _coder;

//    public CoderTool(CoderAgent coder)
//    {
//        _coder = coder;
//    }

//    [Function(
//        functionName: "generate_csharp_code",
//        description: "Generate or fix C# code based on problem and feedback"
//    )]
//    public async Task<CodeArtifact> GenerateAsync(CodeArtifact artifact, string problem, string feedback)
//    {
//        return await _coder.GenerateCodeAsync(artifact,problem, feedback);
//    }
//}
