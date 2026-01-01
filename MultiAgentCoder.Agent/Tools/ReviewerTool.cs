//using AutoGen.Core;
//using MultiAgentCoder.Agents.CoreAgents;
//using MultiAgentCoder.Agents.Tools.Interfaces;

//namespace MultiAgentCoder.Agents.Tools;

///// <summary>
///// Created for the ReviewerAgent to review C# code and provide feedback or approval.
///// Use with AutoGen framework.
///// </summary>
//public class ReviewerTool : IReviewerTool
//{
//    private readonly ReviewerAgent _reviewer;

//    public ReviewerTool(ReviewerAgent reviewer)
//    {
//        _reviewer = reviewer;
//    }

//    [Function(
//        functionName: "review_csharp_code",
//        description: "Review C# code and return feedback or APPROVED"
//    )]
//    public async Task<string> ReviewAsync(string code)
//    {
//        return await _reviewer.ReviewAsync(code);
//    }
//}
