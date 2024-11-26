using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Edi.Translator.Providers.AzureOpenAI;

public interface IAOAIClient
{
    Task<ChatMessageContentPart> TranslateAsync(string fromLang, string toLang, string content, string deploymentName);
}

public class AOAIClient : IAOAIClient
{
    private readonly AzureOpenAIClient _azureClient;

    public AOAIClient(IConfiguration configuration)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"];
        var apiKey = configuration["AzureOpenAI:Key"];

        _azureClient = new(new(endpoint!), new ApiKeyCredential(apiKey!));
        _azureClient = new(new(endpoint!), new ApiKeyCredential(apiKey!));
    }

    public async Task<ChatMessageContentPart> TranslateAsync(string fromLang, string toLang, string content, string deploymentName)
    {
        var chatClient = _azureClient.GetChatClient(deploymentName);

        var systemMessage = new SystemChatMessage("You are a professional translator. I will give you language code like 'zh-CN', 'en-US', and a content. You will help me translate text from one language to another language.");
        var userMessage = new UserChatMessage($"Translate the following text from {fromLang} to {toLang}: {content}");

        var response = await chatClient.CompleteChatAsync(systemMessage, userMessage);

        var firstContent = response?.Value?.Content?.FirstOrDefault();
        return firstContent;
    }
}
