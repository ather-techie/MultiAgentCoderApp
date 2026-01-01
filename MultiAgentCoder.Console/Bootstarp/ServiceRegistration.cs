using AutoGen.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using MultiAgentCoder.Agents.CoreAgents.Dev;
using MultiAgentCoder.Agents.CoreAgents.Interfaces;
using MultiAgentCoder.Agents.CoreAgents.Ops;
using MultiAgentCoder.Agents.CoreAgents.Tests;
using MultiAgentCoder.Agents.RoleAgents;
using MultiAgentCoder.Agents.RoleAgents.Interfaces;
using MultiAgentCoder.Agents.Services;
using MultiAgentCoder.Agents.Services.Interfaces;
using MultiAgentCoder.Console.Cli;
using MultiAgentCoder.Console.Cli.Commands;
using MultiAgentCoder.Console.Cli.Interfaces;
using MultiAgentCoder.Console.Orchestration;
using MultiAgentCoder.Console.Orchestration.Interfaces;

namespace MultiAgentCoder.Console.Bootstarp
{
    public static class ServiceRegistration
    {
        public static IServiceCollection Register(
        this IServiceCollection services)
        {

            // Named HttpClient for LLM calls
            // This client can be injected into services that need to make LLM calls
            // and allows for specific configuration like timeout settings
            services.AddHttpClient("SemanticKernelLLM", client =>
            {
                client.Timeout = TimeSpan.FromMinutes(5);
            });

            // Setup logging to console
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                });
                logging.SetMinimumLevel(LogLevel.Information);
            });

            // Semantic Kernel (Singleton)
            services.AddSingleton<Kernel>(sp =>
            {
                var builder = KernelFactory.CreateKernel(sp);

                return builder.Build();
            });

            // Agents
            services.AddTransient<ICodeWriterAgent, CodeWriterAgent>();
            services.AddTransient<ICodeReviewerAgent, CodeReviewerAgent>();    
            services.AddTransient<IBuildAgent, BuildAgent>();
            services.AddTransient<IProjectScaffoldingAgent, ProjectScaffoldingAgent>();
            services.AddTransient<ITestWriterAgent, TestWriterAgent>();
            services.AddTransient<ITestScaffoldingAgent, TestProjectScaffoldingAgent>();
            services.AddTransient<ITestRunnerAgent, TestRunnerAgent>();

            //Role Agents
            services.AddTransient<IDevAgent, DevAgent>();
            services.AddTransient<IQaAgent, QaAgent>();


            // services
            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IProjectService, ProjectService>();

            // CLI setup
            services.AddTransient<ICliRouter,CliRouter>();
            services.AddTransient<IGenerateCommand,GenerateCommand>();
            services.AddTransient<IRunCommand, RunCommand>();


            // Orchestration
            services.AddSingleton<IHybridOrchestrator, HybridOrchestrator>();

            return services;
        }
        }
}
