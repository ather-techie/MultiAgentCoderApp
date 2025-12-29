using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAgentCoder.Console.Bootstarp
{
    public static class KernelFactory
    {
        public static IKernelBuilder CreateKernel(IServiceProvider serviceProvider)
        {
            var builder = Kernel.CreateBuilder();

            // Build configuration (appsettings.json + environment variables)
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Use Directory.GetCurrentDirectory() for base path
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                //.AddEnvironmentVariables()
                .Build();

            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("SemanticKernelLLM");


            // Read OpenAI settings from configuration
            var modelId = config["OpenAI:ModelId"] ?? "gpt-4o-mini";
            var endpointString = config["OpenAI:Endpoint"] ?? "";
            var apiKey = config["OpenAI:ApiKey"] ?? "";

            var endpoint = new Uri(endpointString);

            builder.AddOpenAIChatCompletion(
                modelId: modelId,
                endpoint: endpoint,
                apiKey: apiKey!,
                httpClient: httpClient                
            );

            return builder;
        }
    }
}
