using Azure;
using Azure.AI.Translation.Text;
using Edi.Translator.Models;
using Microsoft.Extensions.Options;

namespace Edi.Translator.Services;

public interface IAzureTranslatorService
{
    Task<TranslationResult> TranslateAsync(TranslationRequest request, CancellationToken cancellationToken = default);
}

public class AzureTranslatorService : IAzureTranslatorService
{
    private readonly TextTranslationClient _client;
    private readonly ILogger<AzureTranslatorService> _logger;

    public AzureTranslatorService(IOptions<AzureTranslatorConfig> config, ILogger<AzureTranslatorService> logger)
    {
        var cfg = config.Value;
        _client = new TextTranslationClient(
            new AzureKeyCredential(cfg.Key),
            new Uri(cfg.Endpoint),
            cfg.Region);
        _logger = logger;
    }

    public async Task<TranslationResult> TranslateAsync(TranslationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _client.TranslateAsync(
            request.ToLang,
            request.Content,
            request.FromLang,
            cancellationToken: cancellationToken);

        var translations = response.Value;
        if (!translations.Any())
        {
            _logger.LogWarning("No translations returned from Azure Translator service");
            throw new InvalidOperationException("Translation service returned no results");
        }

        var translation = translations.First();
        if (!translation.Translations.Any())
        {
            _logger.LogWarning("No translation text returned from Azure Translator service");
            throw new InvalidOperationException("Translation service returned no translation text");
        }

        _logger.LogInformation("Successfully translated text using Azure Translator. From: {FromLang}, To: {ToLang}",
            request.FromLang ?? "auto-detect", request.ToLang);

        return new TranslationResult
        {
            ProviderCode = "azure-translator",
            TranslatedText = translation.Translations[0].Text,
            DetectedLanguage = translation.DetectedLanguage?.Language,
            Confidence = translation.DetectedLanguage?.Score
        };
    }
}
