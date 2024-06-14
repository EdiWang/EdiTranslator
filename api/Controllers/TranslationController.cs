using Edi.AzureTranslatorProxy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text;
using System.Text.Json;

namespace Edi.AzureTranslatorProxy.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TranslationController(
    IConfiguration configuration,
    ILogger<TranslationController> logger,
    HttpClient httpClient)
    : ControllerBase
{
    [HttpPost("translate")]
    [EnableRateLimiting("TranslateLimiter")]
    public async Task<IActionResult> Translate([FromBody] TranslationRequest request)
    {
        try
        {
            var endpoint = configuration["AzureTranslator:Endpoint"];
            var subscriptionKey = configuration["AzureTranslator:SubscriptionKey"];
            var region = configuration["AzureTranslator:Region"];

            var route = $"/translate?api-version=3.0&from={request.FromLang}&to={request.ToLang}";

            var body = new object[] { new { Text = request.Content } };
            var requestBody = JsonSerializer.Serialize(body);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint + route);
            requestMessage.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            requestMessage.Headers.Add("Ocp-Apim-Subscription-Region", region);

            var response = await httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var responseObject = await response.Content.ReadFromJsonAsync<List<TranslationResponse>>(
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return Ok(responseObject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while translating text.");
            return StatusCode(500, "Internal server error.");
        }
    }
}