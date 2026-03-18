using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.ClientModel;

namespace Edi.Translator.Providers.MicrosoftFoundry;

public interface IFoundryClient
{
    Task<string> TranslateAsync(string? fromLang, string toLang, string content, string deploymentName, CancellationToken cancellationToken = default);
}

public class MicrosoftFoundryOptions
{
    public const string SectionName = "MicrosoftFoundry";

    public required string Endpoint { get; init; }
    public required string Key { get; init; }
    public MicrosoftFoundryDeploymentOption[] Deployments { get; init; } = [];
}

public class MicrosoftFoundryDeploymentOption
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public bool Enabled { get; init; } = true;
}

public class FoundryClient : IFoundryClient
{
    private readonly AzureOpenAIClient _azureClient;
    private readonly MicrosoftFoundryOptions _options;
    private readonly ILogger<FoundryClient> _logger;

    private const string SystemPrompt = """
        You are a professional translator. I will provide you with language codes like 'zh-CN', 'en-US', and content to translate.
        Translate the text accurately while preserving the original meaning, tone, and context.
        Return only the translated text without any additional explanations or formatting.
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

        if (!IsValidDeployment(deploymentName))
        {
            throw new ArgumentException($"Invalid deployment name: {deploymentName}", nameof(deploymentName));
        }

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

    private bool IsValidDeployment(string deploymentName)
    {
        var deployment = _options.Deployments
            .FirstOrDefault(d => d.Name.Equals(deploymentName, StringComparison.OrdinalIgnoreCase));

        return deployment?.Enabled == true;
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
