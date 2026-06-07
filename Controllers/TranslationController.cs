using Azure;
using Edi.Translator.Models;
using Edi.Translator.Providers.MicrosoftFoundry;
using Edi.Translator.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Edi.Translator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranslationController(
    IOptions<MicrosoftFoundryOptions> openAIOptions,
    ILogger<TranslationController> logger,
    IFoundryClient foundryClient,
    IAzureTranslatorService azureTranslatorService)
    : ControllerBase
{
    private readonly MicrosoftFoundryOptions _openAIOptions = openAIOptions.Value;

    [HttpPost("azure-translator")]
    [EnableRateLimiting("TranslateLimiter")]
    public async Task<IActionResult> Translate([FromBody] TranslationRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await azureTranslatorService.TranslateAsync(request, cancellationToken);
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
            var translatedText = await foundryClient.TranslateAsync(
                request.FromLang,
                request.ToLang,
                request.Content,
                deploymentName,
                cancellationToken);

            var result = new TranslationResult
            {
                ProviderCode = "foundry",
                TranslatedText = translatedText
            };

            logger.LogInformation("Successfully translated text using Microsoft Foundry. Deployment: {DeploymentName}, From: {FromLang}, To: {ToLang}",
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
            logger.LogError(ex, "Error occurred while translating text using Microsoft Foundry. Deployment: {DeploymentName}", deploymentName);
            return StatusCode(500, "An unexpected error occurred during translation");
        }
    }

    [HttpPost("ai/{deploymentName}/stream")]
    [EnableRateLimiting("TranslateLimiter")]
    public async Task<IActionResult> TranslateByAzureAIStreaming(
        [FromBody] TranslationRequest request,
        [FromRoute] string deploymentName,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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

        Response.ContentType = "application/x-ndjson; charset=utf-8";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.TryAdd("X-Accel-Buffering", "no");

        try
        {
            await foreach (var text in foundryClient.TranslateStreamingAsync(
                request.FromLang,
                request.ToLang,
                request.Content,
                deploymentName,
                cancellationToken))
            {
                await WriteStreamEventAsync(new { type = "delta", text }, cancellationToken);
            }

            await WriteStreamEventAsync(new { type = "done", providerCode = "foundry" }, cancellationToken);

            logger.LogInformation("Successfully streamed translation using Microsoft Foundry. Deployment: {DeploymentName}, From: {FromLang}, To: {ToLang}",
                deploymentName, request.FromLang, request.ToLang);

            return new EmptyResult();
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("OpenAI streaming translation request was cancelled");

            if (Response.HasStarted)
            {
                return new EmptyResult();
            }

            return StatusCode(408, "Request timeout");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while streaming translation using Microsoft Foundry. Deployment: {DeploymentName}", deploymentName);

            if (Response.HasStarted)
            {
                await WriteStreamEventAsync(new { type = "error", message = "An unexpected error occurred during translation" }, cancellationToken);
                return new EmptyResult();
            }

            return StatusCode(500, "An unexpected error occurred during translation");
        }
    }

    private MicrosoftFoundryDeploymentOption GetDeployment(string deploymentName)
    {
        if (string.IsNullOrWhiteSpace(deploymentName))
        {
            return null;
        }

        return _openAIOptions.Deployments?
            .FirstOrDefault(d => d.Name.Equals(deploymentName, StringComparison.OrdinalIgnoreCase));
    }

    private async Task WriteStreamEventAsync<TEvent>(TEvent streamEvent, CancellationToken cancellationToken)
    {
        var line = JsonSerializer.Serialize(streamEvent);
        await Response.WriteAsync(line, cancellationToken);
        await Response.WriteAsync("\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }
}
