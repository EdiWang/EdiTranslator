using System.Text;
using System.Text.Json;

namespace Edi.Translator.Providers.AzureOpenAI;

public interface IAOAIClient
{
    Task<AOAIResponse> TranslateAsync(string fromLang, string toLang, string content, string deploymentName);
}

public class AOAIClient : IAOAIClient
{
    private readonly HttpClient _httpClient;

    public AOAIClient(
        IConfiguration configuration,
        HttpClient httpClient)
    {
        var configuration1 = configuration;
        var endpoint = configuration1["AzureOpenAI:Endpoint"];
        var apiKey = configuration1["AzureOpenAI:Key"];

        _httpClient = httpClient;
        _httpClient.BaseAddress = new(endpoint!);
        _httpClient.DefaultRequestHeaders.Add("api-key", $"{apiKey}");
    }

    public async Task<AOAIResponse> TranslateAsync(string fromLang, string toLang, string content, string deploymentName)
    {
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

public class AOAIMessage
{
    public string Role { get; set; }

    public string Content { get; set; }
}

public class AOAIRequest
{
    public List<AOAIMessage> Messages { get; set; }
}

public class AOAIResponse
{
    public Choice[] Choices { get; set; }
}

public class Choice
{
    public Message Message { get; set; }
}

public class Message
{
    public string Content { get; set; }
    public string Role { get; set; }
}