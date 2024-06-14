using Azure.AI.Translation.Text;
using Azure;
using Edi.Translator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Edi.Translator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranslationController(
    IConfiguration configuration,
    ILogger<TranslationController> logger)
    : ControllerBase
{
    [HttpPost("translate")]
    [EnableRateLimiting("TranslateLimiter")]
    public async Task<IActionResult> Translate([FromBody] TranslationRequest request)
    {
        try
        {
            var endpoint = configuration["AzureTranslator:Endpoint"];
            var apiKey = configuration["AzureTranslator:Key"];
            var region = configuration["AzureTranslator:Region"];

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
}