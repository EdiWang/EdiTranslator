# Edi's Translator

A simple Web UI that uses the Azure Translator API and Azure Open AI to translate text from one language to another. 

![image](https://github.com/EdiWang/EdiTranslator/assets/3304703/a29edb4e-8d61-4db6-9c85-e1e7c7ecab8e)

## Features

- Translate text from one language to another
- Save translation history
- API Providers
  - Azure Translator (Text)
  - Azure Open AI

## How to Run

### Docker

#### Azure Translator only

```bash
docker run -d -p 8080:8080 -e AzureTranslator__Key=********* -e AzureTranslator__Region==********* ediwang/editranslator
```

#### Azure Open AI only

```bash
docker run -d -p 8080:8080 -e -AzureOpenAI__Endpoint=****** -e AzureOpenAI__Key=********* ediwang/editranslator
```

#### Both

```bash
docker run -d -p 8080:8080 -e AzureTranslator__Key=********* -e AzureTranslator__Region==********* -e AzureOpenAI__Endpoint=****** -e AzureOpenAI__Key=********* ediwang/editranslator
```

### Code Deployment

See `Development` section for setup the project. Then use `Release` configuration to build and deploy to your server.

> Please note there is no built in authentication, if you need your users to login, you will need to deploy an authentication provider in front of the app. For example, you can enable SSO in Azure App Service.

## Development

0. Create an Azure Translator instace and get the API key and region
1. Create an Azure Open AI instance
  - Deploy chat completion models like `gpt-4.1`
  - Get the API key and endpoint
2. Open the solution in Visual Studio
3. Modify `appsettings.json` or create `appsettings.Development.json` and set your API key and region like this

```json
{
  "AzureTranslator": {
    "Endpoint": "https://api.cognitive.microsofttranslator.com",
    "Key": "YOUR_AZURE_TRANSLATOR_KEY",
    "Region": "YOUR_AZURE_TRANSLATOR_REGION"
  },
  "AzureOpenAI": {
    "Endpoint": "https://<your_instance>.openai.azure.com/",
    "Key": "YOUR_AZURE_OPENAI_KEY",
    "Deployments": [
      {
        "Name": "gpt-4.1",
        "DisplayName": "GPT-4.1 (Azure)",
        "Enabled": true
      },
      {
        "Name": "gpt-4.1-mini",
        "DisplayName": "GPT-4.1-mini (Azure)",
        "Enabled": false
      },
      {
        "Name": "gpt-5-mini",
        "DisplayName": "GPT-5-mini (Azure)",
        "Enabled": true
      },
      {
        "Name": "gpt-5.1-chat",
        "DisplayName": "GPT-5.1-chat (Azure)",
        "Enabled": true
      },
      {
        "Name": "gpt-5.2-chat",
        "DisplayName": "GPT-5.2-chat (Azure)",
        "Enabled": true
      },
      {
        "Name": "DeepSeek-V3.2",
        "DisplayName": "DeepSeek-V3.2 (Azure)",
        "Enabled": true
      }
    ]
  }
}

```

4. Run the project

## Tech Stack

- AI: Azure Translator API, Azure Open AI
- Frontend: ASP.NET Core Razor Page
- Backend: ASP.NET Core Web API
