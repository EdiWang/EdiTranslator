using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.ClientModel;

namespace Edi.Translator.Providers.MicrosoftFoundry;

public interface IFoundryClient
{
    Task<string> TranslateAsync(string? fromLang, string toLang, string content, string deploymentName, CancellationToken cancellationToken = default);
}

public class FoundryClient : IFoundryClient
{
    private readonly AzureOpenAIClient _azureClient;
    private readonly MicrosoftFoundryOptions _options;
    private readonly ILogger<FoundryClient> _logger;

    private const string SystemPrompt = """
        You are a professional translator with expertise in multiple languages.
        You will receive BCP-47 language codes (e.g., 'zh-CN', 'en-US') and text to translate.

        Rules:
        - Translate accurately, preserving the original meaning, tone, style, and context.
        - Preserve all formatting, including punctuation, line breaks, markdown syntax, and HTML tags.
        - Do not translate proper nouns, brand names, product names, or technical identifiers unless a widely accepted localized form exists.
        - Do not add explanations, notes, alternatives, or any content not present in the original text.
        - Do not omit any part of the original text.
        - If the source and target language are identical, return the original text unchanged.
        - Return only the translated text, with no preamble or commentary.
        """;

    public FoundryClient(IOptions<MicrosoftFoundryOptions> options, ILogger<FoundryClient> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;

        ValidateConfiguration(_options);

        _azureClient = new AzureOpenAIClient(new Uri(_options.Endpoint), new ApiKeyCredential(_options.Key));
    }

    public async Task<string> TranslateAsync(
        string? fromLang,
        string toLang,
        string content,
        string deploymentName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toLang);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(deploymentName);

        try
        {
            var chatClient = _azureClient.GetChatClient(deploymentName);

            var systemMessage = new SystemChatMessage(SystemPrompt);

            var prompt = string.IsNullOrWhiteSpace(fromLang)
                ? $"Auto-detect the source language and translate the following text to {toLang}: {content}"
                : $"Translate the following text from {fromLang} to {toLang}: {content}";

            var userMessage = new UserChatMessage(prompt);

            var response = await chatClient.CompleteChatAsync(
                [systemMessage, userMessage],
                cancellationToken: cancellationToken);

            var firstContent = response?.Value?.Content?.FirstOrDefault();

            if (firstContent is null)
            {
                _logger.LogWarning("Received empty response from Microsoft Foundry for deployment {DeploymentName}", deploymentName);
                return string.Empty;
            }

            return firstContent.Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error translating text using deployment {DeploymentName}", deploymentName);
            throw;
        }
    }

    private static void ValidateConfiguration(MicrosoftFoundryOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            throw new InvalidOperationException("MicrosoftFoundry:Endpoint configuration is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Key))
        {
            throw new InvalidOperationException("MicrosoftFoundry:Key configuration is required.");
        }

        if (!Uri.TryCreate(options.Endpoint, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("MicrosoftFoundry:Endpoint must be a valid URI.");
        }
    }
}
