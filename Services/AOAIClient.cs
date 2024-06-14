using Edi.Translator.Models;
using System.Text.Json;
using System.Text;

namespace Edi.Translator.Services;

public interface IAOAIClient
{
    Task<AOAIResponse> TranslateAsync(string fromLang, string toLang, string content);
}

public class AOAIClient : IAOAIClient
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    
    public AOAIClient(
        IConfiguration configuration, 
        HttpClient httpClient)
    {
        _configuration = configuration;
        var endpoint = _configuration["AzureOpenAI:Endpoint"];
        var apiKey = _configuration["AzureOpenAI:Key"];

        _httpClient = httpClient;
        _httpClient.BaseAddress = new(endpoint!);
        _httpClient.DefaultRequestHeaders.Add("api-key", $"{apiKey}");
    }

    public async Task<AOAIResponse> TranslateAsync(string fromLang, string toLang, string content)
    {
        var deploymentName = _configuration["AzureOpenAI:DeploymentName"];

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
                        Content = $"Translate the following text from {fromLang} to {toLang}: {content}"
                    }
            ]
        };

        var request = new StringContent(JsonSerializer.Serialize(requestBody, AOAISerializeOption.Default), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"/openai/deployments/{deploymentName}/chat/completions?api-version=2024-02-15-preview", request);
        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AOAIResponse>(responseBody, AOAISerializeOption.Default);

        return result;
    }
}