using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using MultiAgentCoder.Agents.Agents;
using MultiAgentCoder.Agents.Agents.Interfaces;
using MultiAgentCoder.Agents.Services;
using MultiAgentCoder.Agents.Services.Interfaces;
using MultiAgentCoder.Console.Orchestration;
using MultiAgentCoder.Console.Orchestration.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

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
            services.AddTransient<ICoderWritterAgent, CodeWritterrAgent>();
            services.AddTransient<IReviewerAgent, ReviewerAgent>();    
            services.AddTransient<IBuildAgent, BuildAgent>();
            services.AddTransient<IProjectScaffoldingAgent, ProjectScaffoldingAgent>();
            services.AddTransient<ITestWriterAgent, TestWriterAgent>();
            services.AddTransient<ITestScaffoldingAgent, TestProjectScaffoldingAgent>();
            services.AddTransient<ITestRunnerAgent, TestRunnerAgent>();

            // services
            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IProjectService, ProjectService>();


            // Orchestration
            services.AddSingleton<IHybridOrchestrator, HybridOrchestrator>();

            return services;
        }
        }
}
