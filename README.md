# Edi's Translator

A simple Web UI that uses Microsoft Foundry chat completions to translate text from one language to another. 

![image](https://github.com/EdiWang/EdiTranslator/assets/3304703/a29edb4e-8d61-4db6-9c85-e1e7c7ecab8e)

## Features

- Translate text from one language to another
- Save translation history
- Microsoft Foundry deployment selection
- Optional Microsoft account SSO with an email allow-list

## How to Run

### Docker

```bash
docker run -d -p 8080:8080 -e MicrosoftFoundry__Endpoint=****** -e MicrosoftFoundry__Key=********* ediwang/editranslator
```

To enable Microsoft account SSO in Docker, pass authentication settings as environment variables:

```bash
docker run -d -p 8080:8080 \
  -e MicrosoftFoundry__Endpoint=****** \
  -e MicrosoftFoundry__Key=********* \
  -e Authentication__Enabled=true \
  -e Authentication__AllowedEmails__0=you@example.com \
  -e Authentication__Providers__Microsoft__ClientId=****** \
  -e Authentication__Providers__Microsoft__ClientSecret=****** \
  ediwang/editranslator
```

### Code Deployment

See `Development` section for setup the project. Then use `Release` configuration to build and deploy to your server.

## Development

1. Create a Microsoft Foundry project
  - Deploy chat completion models like `gpt-5.5`
  - Get the API key and endpoint
2. Open the solution in Visual Studio
3. Modify `appsettings.json` or create `appsettings.Development.json` and set your Microsoft Foundry API key and endpoint like this

```json
{
  "MicrosoftFoundry": {
    "Endpoint": "https://<your_instance>.openai.azure.com/",
    "Key": "YOUR_AZURE_OPENAI_KEY",
    "Deployments": [
      {
        "Name": "gpt-4.1",
        "DisplayName": "GPT-4.1 (Azure)",
        "Enabled": true
      },
      {
        "Name": "gpt-5.4",
        "DisplayName": "GPT-5.4 (Azure)",
        "Enabled": true
      },
      {
        "Name": "gpt-5.5",
        "DisplayName": "GPT-5.5 (Azure)",
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

## Authentication

Authentication is disabled by default. When enabled, the whole site and all `/api/*` endpoints require sign-in. Phase one supports personal Microsoft accounts only, and access is limited to the email addresses in `Authentication:AllowedEmails`.

```json
{
  "Authentication": {
    "Enabled": true,
    "AllowedEmails": [
      "you@example.com"
    ],
    "Providers": {
      "Microsoft": {
        "Enabled": true,
        "ClientId": "YOUR_MICROSOFT_CLIENT_ID",
        "ClientSecret": "YOUR_MICROSOFT_CLIENT_SECRET",
        "CallbackPath": "/signin-microsoft"
      }
    }
  }
}
```

For local development, store `ClientId` and `ClientSecret` with User Secrets or `appsettings.Development.json`. For production, prefer environment variables or a managed secret store.

Microsoft app registration setup:

1. Create an app registration in the Microsoft Entra admin center.
2. Choose **Personal Microsoft accounts only** for supported account types.
3. Add a **Web** redirect URI that matches the app host plus the callback path, for example `https://localhost:5001/signin-microsoft` locally or `https://your-domain.example/signin-microsoft` in production.
4. Create a client secret.
5. Copy the application client ID and client secret into configuration.

## Tech Stack

- AI: Microsoft Foundry
- Frontend: ASP.NET Core Razor Page
- Backend: ASP.NET Core Web API
