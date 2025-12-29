//using AutoGen;
//using MultiAgentCoder.Agents.Tools.Interfaces;
//using AutoGen.OpenAI;
//using AutoGen.Core;
//using OpenAI.Responses;

//namespace MultiAgentCoder.Agents.Agents;


//public class AutoGenOrchestrator
//{
//    private readonly AssistantAgent _agent;
//    private readonly ICoderTool _coderTool;
//    private readonly IReviewerTool _reviewerTool;

//    public AutoGenOrchestrator(
//        ICoderTool coderTool,
//        IReviewerTool reviewerTool,
//        string apiKey)
//    {
//        _coderTool = coderTool ?? throw new ArgumentNullException(nameof(coderTool));
//        _reviewerTool = reviewerTool ?? throw new ArgumentNullException(nameof(reviewerTool));

//        _agent = new AssistantAgent(
//            name: "Orchestrator",
//            systemMessage: """
//            You orchestrate a multi-agent C# coding workflow.

//            Steps:
//            1. Ask coder tool to generate code
//            2. Ask reviewer tool to review code
//            3. If reviewer says APPROVED, stop
//            4. Otherwise send feedback back to coder
//            """
//            );

//        var weatherTool = new FunctionTool(
//    name: "get_weather",
//    description: "Get the weather for a city",
//    function: GetWeather
//);

//        var coderToolMiddleware = new = new FunctionCallMiddleware(
//            functions: [_coderTool.GenerateAsync],
//            functionMap: new Dictionary<string, Func<string, Task<string>>>
//            {
//                { "get_weather", (args) => GetWeather(args) }
//            }


//        );        var reviewerToolMiddleware = new FunctionCallMiddleware(
//            functions: [reviewerTool.GetFunctionContract()],
//            functionMap: new Dictionary<string, Func<string, Task<string>>>
//            {
//                { "review_code", (args) => reviewerTool.ReviewCodeAsync(args) }
//            }
//        );

//        // Tools are kept locally and used by this orchestrator when needed.
//    }

//    public async Task RunAsync(string problem)
//    {
//        var message = new TextMessage(Role.User,
//        $"""
//        Solve the following problem using available tools:

//        {problem}
//        """
//        );

//        await _agent.SendAsync(message);
//    }
//}


