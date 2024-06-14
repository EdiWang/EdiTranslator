# Edi's Translator

A simple Web UI that uses the Azure Translator API and Azure Open AI to translate text from one language to another. 

![image](https://github.com/EdiWang/EdiTranslator/assets/3304703/8b3de68c-f6aa-46c2-8ca9-a534d878d111)

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
docker run -d -p 8080:8080 -e -AzureOpenAI__Endpoint=****** -e AzureOpenAI__Key=********* -e AzureOpenAI__DeploymentName=gpt-4o ediwang/editranslator
```

#### Both

```bash
docker run -d -p 8080:8080 -e AzureTranslator__Key=********* -e AzureTranslator__Region==********* -e AzureOpenAI__Endpoint=****** -e AzureOpenAI__Key=********* -e AzureOpenAI__DeploymentName=gpt-4o ediwang/editranslator
```

### Code Deployment

See `Development` section for setup the project. Then use `Release` configuration to build and deploy to your server.

> Please note there is no built in authentication, if you need your users to login, you will need to deploy an authentication provider in front of the app. For example, you can enable SSO in Azure App Service.

## Development

0. Create an Azure Translator instace and get the API key and region
1. Create an Azure Open AI instance, deploy a model and get the API key and endpoint
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
    "DeploymentName": "gpt-4o"
  },
}
```

4. Run the project

## Tech Stack

- AI: Azure Translator API, Azure Open AI
- Frontend: Angular
- Backend: ASP.NET Core

## 免责申明

对于中国访客，我们有一份特定的免责申明。请确保你已经阅读并理解其内容：[免责申明（仅限中国访客）](./DISCLAIMER_CN.md)