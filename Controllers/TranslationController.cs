using Azure;
using Azure.AI.Translation.Text;
using Edi.Translator.Models;
using Edi.Translator.Services;
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

            return Ok(translation);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while translating text.");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("aoai")]
    [EnableRateLimiting("TranslateLimiter")]
    public async Task<IActionResult> TranslateByOpenAI([FromBody] TranslationRequest request)
    {
        try
        {
            var result = await aoaiClient.TranslateAsync(request.FromLang, request.ToLang, request.Content);

            return Ok(result.Choices[0]?.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while translating text.");
            return StatusCode(500, ex.Message);
        }
    }
}