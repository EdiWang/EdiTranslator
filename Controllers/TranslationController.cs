using Edi.Translator.Models;
using Edi.Translator.Providers.MicrosoftFoundry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Edi.Translator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranslationController(
    IOptions<MicrosoftFoundryOptions> foundryOptions,
    ILogger<TranslationController> logger,
    IFoundryClient foundryClient)
    : ControllerBase
{
    private readonly MicrosoftFoundryOptions _foundryOptions = foundryOptions.Value;

    [HttpPost("{deploymentName}")]
    [EnableRateLimiting("TranslateLimiter")]
    public async Task<IActionResult> Translate(
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
                DeploymentName = deploymentName,
                TranslatedText = translatedText
            };

            logger.LogInformation("Successfully translated text using Microsoft Foundry. Deployment: {DeploymentName}, From: {FromLang}, To: {ToLang}",
                deploymentName, request.FromLang, request.ToLang);

            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Microsoft Foundry translation request was cancelled");
            return StatusCode(408, "Request timeout");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while translating text using Microsoft Foundry. Deployment: {DeploymentName}", deploymentName);
            return StatusCode(500, "An unexpected error occurred during translation");
        }
    }

    [HttpPost("{deploymentName}/stream")]
    [EnableRateLimiting("TranslateLimiter")]
    public async Task<IActionResult> TranslateStreaming(
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

            await WriteStreamEventAsync(new { type = "done", deploymentName }, cancellationToken);

            logger.LogInformation("Successfully streamed translation using Microsoft Foundry. Deployment: {DeploymentName}, From: {FromLang}, To: {ToLang}",
                deploymentName, request.FromLang, request.ToLang);

            return new EmptyResult();
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Microsoft Foundry streaming translation request was cancelled");

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

        return _foundryOptions.Deployments?
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
