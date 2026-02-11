using Azure;
using Azure.AI.Translation.Text;
using Edi.Translator.Models;
using Edi.Translator.Providers.AzureOpenAI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Edi.Translator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranslationController(
    IOptions<AzureTranslatorConfig> translatorConfig,
    IOptions<AzureOpenAIOptions> openAIOptions,
    ILogger<TranslationController> logger,
    IAOAIClient aoaiClient)
    : ControllerBase
{
    private readonly AzureTranslatorConfig _translatorConfig = translatorConfig.Value;
    private readonly AzureOpenAIOptions _openAIOptions = openAIOptions.Value;

    [HttpPost("azure-translator")]
    [EnableRateLimiting("TranslateLimiter")]
    public async Task<IActionResult> Translate([FromBody] TranslationRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validate configuration
        var configValidation = ValidateTranslatorConfiguration();
        if (configValidation != null)
        {
            return configValidation;
        }

        try
        {
            var client = new TextTranslationClient(
                new AzureKeyCredential(_translatorConfig.Key),
                new Uri(_translatorConfig.Endpoint),
                _translatorConfig.Region);

            var response = await client.TranslateAsync(
                request.ToLang,
                request.Content,
                request.FromLang,
                cancellationToken: cancellationToken);

            var translations = response.Value;
            if (!translations.Any())
            {
                logger.LogWarning("No translations returned from Azure Translator service");
                return StatusCode(500, "Translation service returned no results");
            }

            var translation = translations.First();
            if (!translation.Translations.Any())
            {
                logger.LogWarning("No translation text returned from Azure Translator service");
                return StatusCode(500, "Translation service returned no translation text");
            }

            var result = new TranslationResult
            {
                ProviderCode = "azure-translator",
                TranslatedText = translation.Translations[0].Text,
                DetectedLanguage = translation.DetectedLanguage?.Language,
                Confidence = translation.DetectedLanguage?.Confidence
            };

            logger.LogInformation("Successfully translated text using Azure Translator. From: {FromLang}, To: {ToLang}",
                request.FromLang ?? "auto-detect", request.ToLang);

            return Ok(result);
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "Azure Translator API request failed. Status: {Status}, Error: {Error}",
                ex.Status, ex.ErrorCode);
            return StatusCode(503, "Translation service is temporarily unavailable");
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Translation request was cancelled");
            return StatusCode(408, "Request timeout");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while translating text");
            return StatusCode(500, "An unexpected error occurred during translation");
        }
    }

    [HttpPost("ai/{deploymentName}")]
    [EnableRateLimiting("TranslateLimiter")]
    public async Task<IActionResult> TranslateByAzureAI(
        [FromBody] TranslationRequest request,
        [FromRoute] string deploymentName,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validate deployment name
        var deployment = GetDeployment(deploymentName);
        if (deployment == null)
        {
            logger.LogWarning("Invalid deployment name requested: {DeploymentName}", deploymentName);
            return BadRequest($"Invalid deployment name: {deploymentName}");
        }

        if (!deployment.Enabled)
        {
            logger.LogWarning("Deployment disabled: {DeploymentName}", deploymentName);
            return StatusCode(StatusCodes.Status403Forbidden, $"Deployment disabled: {deploymentName}");
        }

        try
        {
            var translatedText = await aoaiClient.TranslateAsync(
                request.FromLang,
                request.ToLang,
                request.Content,
                deploymentName,
                cancellationToken);

            var result = new TranslationResult
            {
                ProviderCode = "aoai",
                TranslatedText = translatedText
            };

            logger.LogInformation("Successfully translated text using Azure OpenAI. Deployment: {DeploymentName}, From: {FromLang}, To: {ToLang}",
                deploymentName, request.FromLang, request.ToLang);

            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("OpenAI translation request was cancelled");
            return StatusCode(408, "Request timeout");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while translating text using Azure OpenAI. Deployment: {DeploymentName}", deploymentName);
            return StatusCode(500, "An unexpected error occurred during translation");
        }
    }

    private ObjectResult ValidateTranslatorConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_translatorConfig.Endpoint) ||
            string.IsNullOrWhiteSpace(_translatorConfig.Key) ||
            string.IsNullOrWhiteSpace(_translatorConfig.Region))
        {
            const string message = "Azure Translator configuration is incomplete";
            logger.LogError(message);
            return StatusCode(500, message);
        }

        return null;
    }

    private AzureOpenAIDeploymentOption GetDeployment(string deploymentName)
    {
        if (string.IsNullOrWhiteSpace(deploymentName))
        {
            return null;
        }

        return _openAIOptions.Deployments?
            .FirstOrDefault(d => d.Name.Equals(deploymentName, StringComparison.OrdinalIgnoreCase));
    }
}
