using Azure;
using Azure.AI.Translation.Text;
using Edi.Translator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text;
using System.Text.Json;

namespace Edi.Translator.Controllers;

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

    [HttpPost("translate/oai")]
    [EnableRateLimiting("TranslateLimiter")]
    public async Task<IActionResult> TranslateByOpenAI([FromBody] TranslationRequest request)
    {
        // TODO: Refact to typed http client
        try
        {
            var endpoint = configuration["AzureOpenAI:Endpoint"];
            var apiKey = configuration["AzureOpenAI:Key"];
            var deploymentName = configuration["AzureOpenAI:DeploymentName"];

            httpClient.DefaultRequestHeaders.Add("api-key", $"{apiKey}");

            var requestBody = new AOAIRequest
            {
                Messages =
                [
                    new()
                    {
                        Role = "system",
                        Content = "You are a professional translator. I will give you language code like 'zh-CN', 'en-US', and a content. You will help me translate text from one language to another language."
                    },

                    new()
                    {
                        Role = "user",
                        Content = $"Translate the following text from {request.FromLang} to {request.ToLang}: {request.Content}"
                    }
                ]
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody, AOAISerializeOption.Default), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync($"{endpoint}/openai/deployments/{deploymentName}/chat/completions?api-version=2024-02-15-preview", content);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AOAIResponse>(responseBody, AOAISerializeOption.Default);

            return Ok(result.Choices[0]?.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while translating text.");
            return StatusCode(500, ex.Message);
        }
    }
}