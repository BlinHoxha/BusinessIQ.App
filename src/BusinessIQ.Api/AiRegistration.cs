using Azure.Identity;
using BuildingBlocks.AI.Azure;
using BuildingBlocks.AI.Core.Abstractions;
using BuildingBlocks.AI.Local;

namespace BusinessIQ.Api;

internal static class AiRegistration
{
    public static void AddBusinessAi(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var provider = configuration["AI:Provider"] ?? "Local";
        services.AddSingleton<ITextChunker, FixedSizeTextChunker>();
        if (string.Equals(provider, "Local", StringComparison.OrdinalIgnoreCase))
        {
            if (!environment.IsDevelopment())
                throw new InvalidOperationException("The Local AI provider is only available in Development.");
            services.AddSingleton<InMemoryKnowledgeStore>();
            services.AddSingleton<IKnowledgeIndex>(sp => sp.GetRequiredService<InMemoryKnowledgeStore>());
            services.AddSingleton<IKnowledgeRetriever>(sp => sp.GetRequiredService<InMemoryKnowledgeStore>());
            services.AddSingleton<IChatModel, LocalGroundedChatModel>();
        }
        else if (string.Equals(provider, "Azure", StringComparison.OrdinalIgnoreCase))
        {
            var section = configuration.GetSection("AI:Azure");
            services.AddSingleton(AzureAiOptions.FromValues(section["OpenAiEndpoint"], section["ChatDeployment"],
                section["EmbeddingDeployment"], section["SearchEndpoint"], section["SearchIndex"]));
            services.AddHttpClient("AzureAI");
            services.AddSingleton<AzureTokenClient>(sp => new AzureTokenClient(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient("AzureAI"), new DefaultAzureCredential()));
            services.AddSingleton<AzureOpenAiClient>();
            services.AddSingleton<IChatModel>(sp => sp.GetRequiredService<AzureOpenAiClient>());
            services.AddSingleton<AzureSearchKnowledgeStore>();
            services.AddSingleton<IKnowledgeIndex>(sp => sp.GetRequiredService<AzureSearchKnowledgeStore>());
            services.AddSingleton<IKnowledgeRetriever>(sp => sp.GetRequiredService<AzureSearchKnowledgeStore>());
        }
        else throw new InvalidOperationException($"Unknown AI provider '{provider}'.");
    }
}

