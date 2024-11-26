using Azure;
using Azure.AI.Translation.Text;
using Edi.Translator.Models;
using Edi.Translator.Providers.AzureOpenAI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Edi.Translator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranslationController(
    IConfiguration configuration,
    ILogger<TranslationController> logger,
    IAOAIClient aoaiClient)
    : ControllerBase
{
    [HttpPost("azure-translator")]
    [EnableRateLimiting("TranslateLimiter")]
    public async Task<IActionResult> Translate([FromBody] TranslationRequest request)
    {
        try
        {
            var endpoint = configuration["AzureTranslator:Endpoint"];
            var apiKey = configuration["AzureTranslator:Key"];
            var region = configuration["AzureTranslator:Region"];

            // check if endpoint, apiKey and region are set
            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(region))
            {
                var message = "Translator configuration is missing.";

                logger.LogError(message);
                return StatusCode(500, message);
            }

            var client = new TextTranslationClient(new AzureKeyCredential(apiKey), new(endpoint), region);
            var response = await client.TranslateAsync(request.ToLang, request.Content, request.FromLang);

            var translations = response.Value;
            var translation = translations.FirstOrDefault();

            var result = new TranslationResult
            {
                ProviderCode = "azure-translator",
                TranslatedText = translation?.Translations[0].Text
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while translating text.");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("aoai/{deploymentName}")]
    [EnableRateLimiting("TranslateLimiter")]
    public async Task<IActionResult> TranslateByOpenAI([FromBody] TranslationRequest request, string deploymentName)
    {
        var deploymentNames = configuration.GetSection("AzureOpenAI:DeploymentNames").Get<string[]>();
        if (!deploymentNames.Contains(deploymentName))
        {
            var message = "Invalid deployment name.";

            logger.LogError(message);
            return BadRequest(message);
        }

        try
        {
            var aoiResult = await aoaiClient.TranslateAsync(request.FromLang, request.ToLang, request.Content, deploymentName);

            var result = new TranslationResult
            {
                ProviderCode = "aoai",
                TranslatedText = aoiResult.Text
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while translating text.");
            return StatusCode(500, ex.Message);
        }
    }
}