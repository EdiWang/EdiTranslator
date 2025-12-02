# Edi's Translator

A simple Web UI that uses the Azure Translator API and Azure Open AI to translate text from one language to another. 

![image](https://github.com/EdiWang/EdiTranslator/assets/3304703/a29edb4e-8d61-4db6-9c85-e1e7c7ecab8e)

## Features

- Translate text from one language to another
- Save translation history
- API Providers
  - Azure Translator (Text)
  - Azure Open AI (GPT-4o and GPT-3.5 Turbo)

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
  - Deploy both GPT-4o and GPT 3.5 Turbo model with deployment name as `gpt-4o` and `gpt-35-turbo`
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
  }
}
```

4. Run the project

## Tech Stack

- AI: Azure Translator API, Azure Open AI
- Frontend: ASP.NET Core Razor Page
- Backend: ASP.NET Core Web API
