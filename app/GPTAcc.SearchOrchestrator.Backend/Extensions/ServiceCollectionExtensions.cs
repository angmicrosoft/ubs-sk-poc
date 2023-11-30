
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using OpenAI =  Microsoft.SemanticKernel.OpenAIKernelBuilderExtensions;

namespace GPTAcc.SearchOrchestrator.Backend.Extensions;

internal static class ServiceCollectionExtensions
{
    private static readonly DefaultAzureCredential s_azureCredential = new();

    internal static IServiceCollection AddAzureServices(this IServiceCollection services)
    {   

        services.AddSingleton<SearchClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var (azureSearchServiceEndpoint, azureSearchIndex) =
                (config["AzureSearchServiceEndpoint"], config["AzureSearchIndex"]);

            ArgumentNullException.ThrowIfNullOrEmpty(azureSearchServiceEndpoint);

            var searchClient = new SearchClient(
                new Uri(azureSearchServiceEndpoint), azureSearchIndex, s_azureCredential);

            return searchClient;
        });

        services.AddSingleton<OpenAIClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var azureOpenAiServiceEndpoint = config["AzureOpenAiServiceEndpoint"];

            ArgumentNullException.ThrowIfNullOrEmpty(azureOpenAiServiceEndpoint);

            var openAIClient = new OpenAIClient(
                new Uri(azureOpenAiServiceEndpoint), s_azureCredential);

            return openAIClient;
        });

        // Semantic Kernel

        services.AddSingleton<IKernel>(
            sp =>
            {
                var client = sp.GetRequiredService<OpenAIClient>();
                var config = sp.GetRequiredService<IConfiguration>();
                var azureOpenAiServiceEndpoint = config["AzureOpenAiServiceEndpoint"];
                ArgumentNullException.ThrowIfNullOrEmpty(azureOpenAiServiceEndpoint);
                var deployedModelName = config["AzureOpenAiChatGptDeployment"];
                ArgumentNullException.ThrowIfNullOrWhiteSpace(deployedModelName);
                var embeddingModelName = config["AzureOpenAiEmbeddingDeployment"];
                ArgumentNullException.ThrowIfNullOrWhiteSpace(embeddingModelName);
                var kernelBuilder = new KernelBuilder();
                kernelBuilder.WithAzureOpenAIChatCompletionService(deployedModelName, client);
                kernelBuilder.WithAzureOpenAITextEmbeddingGenerationService(embeddingModelName, azureOpenAiServiceEndpoint, new DefaultAzureCredential());
                
                var kernel = kernelBuilder.Build();                
                return kernel;
            });

        services.AddSingleton<ChatService>();

        return services;
    }

    internal static IServiceCollection AddCrossOriginResourceSharing(this IServiceCollection services)
    {
        services.AddCors(
            options =>
                options.AddDefaultPolicy(
                    policy =>
                        policy.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod()));

        return services;
    }
}
