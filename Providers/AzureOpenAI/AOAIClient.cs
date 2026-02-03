using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.ClientModel;

namespace Edi.Translator.Providers.AzureOpenAI;

public interface IAOAIClient
{
    Task<ChatMessageContentPart> TranslateAsync(string fromLang, string toLang, string content, string deploymentName, CancellationToken cancellationToken = default);
}

public class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";

    public required string Endpoint { get; init; }
    public required string Key { get; init; }
    public AzureOpenAIDeploymentOption[] Deployments { get; init; } = [];
}

public class AzureOpenAIDeploymentOption
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
}

public class AOAIClient : IAOAIClient
{
    private readonly AzureOpenAIClient _azureClient;
    private readonly AzureOpenAIOptions _options;
    private readonly ILogger<AOAIClient> _logger;

    private const string SystemPrompt = """
        You are a professional translator. I will provide you with language codes like 'zh-CN', 'en-US', and content to translate.
        Translate the text accurately while preserving the original meaning, tone, and context.
        Return only the translated text without any additional explanations or formatting.
        """;

    public AOAIClient(IOptions<AzureOpenAIOptions> options, ILogger<AOAIClient> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;

        ValidateConfiguration(_options);

        _azureClient = new AzureOpenAIClient(new Uri(_options.Endpoint), new ApiKeyCredential(_options.Key));
    }

    public async Task<ChatMessageContentPart> TranslateAsync(
        string fromLang,
        string toLang,
        string content,
        string deploymentName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fromLang);
        ArgumentException.ThrowIfNullOrWhiteSpace(toLang);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(deploymentName);

        if (!IsValidDeployment(deploymentName))
        {
            throw new ArgumentException($"Invalid deployment name: {deploymentName}", nameof(deploymentName));
        }

        try
        {
            var chatClient = _azureClient.GetChatClient(deploymentName);

            var systemMessage = new SystemChatMessage(SystemPrompt);
            var userMessage = new UserChatMessage($"Translate the following text from {fromLang} to {toLang}: {content}");

            var response = await chatClient.CompleteChatAsync(
                [systemMessage, userMessage],
                cancellationToken: cancellationToken);

            var firstContent = response?.Value?.Content?.FirstOrDefault();

            if (firstContent == null)
            {
                _logger.LogWarning("Received empty response from Azure OpenAI for deployment {DeploymentName}", deploymentName);
            }

            return firstContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error translating text using deployment {DeploymentName}", deploymentName);
            throw;
        }
    }

    private bool IsValidDeployment(string deploymentName)
    {
        return _options.Deployments.Length == 0 || 
               _options.Deployments.Any(d => d.Name.Equals(deploymentName, StringComparison.OrdinalIgnoreCase));
    }

    private static void ValidateConfiguration(AzureOpenAIOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            throw new InvalidOperationException("AzureOpenAI:Endpoint configuration is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Key))
        {
            throw new InvalidOperationException("AzureOpenAI:Key configuration is required.");
        }

        if (!Uri.TryCreate(options.Endpoint, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("AzureOpenAI:Endpoint must be a valid URI.");
        }
    }
}
