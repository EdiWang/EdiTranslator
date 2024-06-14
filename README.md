# Edi's Translator

A simple Web UI that uses the Azure AI Translator API to translate text from one language to another. 

> Please note the Azure AI Translator is NOT LLM. If you are looking for LLM (e.g. GPT, llama), this is not for you yet.

![image](https://github.com/EdiWang/EdiTranslator/assets/3304703/8b3de68c-f6aa-46c2-8ca9-a534d878d111)

## Features

- Translate text from one language to another
- Save translation history

## How to Run

### Docker

```bash
docker run -d -p 8080:8080 -e AzureTranslator__SubscriptionKey=********* -e AzureTranslator__Region==********* ediwang/editranslator
```

### Code Deployment

See `Development` section for setup the project. Then use `Release` configuration to build and deploy to your server.

> Please note there is no built in authentication, if you need your users to login, you will need to deploy an authentication provider in front of the app. For example, you can enable SSO in Azure App Service.

## Development

0. Create an Azure Translator instace and get the API key and region
1. Clone the repository
2. Open the solution in Visual Studio
3. Create `appsettings.Development.json` and set your API key and region like this

```json
{
  "AzureTranslator": {
    "SubscriptionKey": "******",
    "Region": "****"
  }
}
```

4. Run the project

## Tech Stack

- AI: Azure Translator API
- Frontend: Angular
- Backend: ASP.NET Core
