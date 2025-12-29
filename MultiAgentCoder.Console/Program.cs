using Microsoft.Extensions.DependencyInjection;
using MultiAgentCoder.Console.Bootstarp;
using MultiAgentCoder.Console.Orchestration;
using MultiAgentCoder.Console.Orchestration.Interfaces;

Console.WriteLine("Starting Hybrid Agentic System...");

var services = new ServiceCollection();

services.Register();

var provider = services.BuildServiceProvider();

// 3. Define problem statement
Console.Write("Please enter problem statement: ");
var problemStatement = Console.ReadLine() ?? string.Empty;
//var problemStatement = """
//            Create a thread-safe in-memory cache in C#
//            with expiration support and unit tests.
//            """;

// 4. Start workflow

var orchestrator = provider.GetRequiredService<IHybridOrchestrator>();
var result = await orchestrator.ExecuteAsync(problemStatement);

// 5. Final outcome
Console.WriteLine(result.Success
    ? "Workflow completed successfully"
    : "Workflow failed");

Console.WriteLine(result.Summary);



// Wait for user input before closing
Console.WriteLine("Press any key to exit...");
Console.ReadKey();