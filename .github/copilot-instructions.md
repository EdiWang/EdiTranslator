# Project Guidelines

## Project Shape

This is a .NET 10 ASP.NET Core web app for text translation. It serves a Razor Pages UI and Web API endpoints from the same project.

- Keep server code under the existing namespaces rooted at `Edi.Translator`.
- The UI is Razor Pages plus plain JavaScript and Fluent UI Web Components loaded from CDN; do not introduce a SPA framework or frontend build pipeline unless explicitly requested.
- The app currently has no built-in authentication. Do not assume users are authenticated; deployment can place SSO or auth in front of the app.

## Architecture

- `Program.cs` wires services, strongly typed options, route casing, static files, rate limiting, Razor Pages, and controllers.
- `Controllers/TranslationController.cs` owns HTTP API behavior under `/api/translation/*` and should stay thin: validate input, select provider/deployment, call services, map exceptions to HTTP responses, and log with structured properties.
- `Services/AzureTranslatorService.cs` wraps Azure Translator SDK calls and returns `TranslationResult`.
- `Providers/MicrosoftFoundry/FoundryClient.cs` wraps Azure OpenAI/Microsoft Foundry chat completions. Preserve the system prompt rules when adjusting AI translation behavior.
- `Pages/Index.cshtml.cs` provides language and provider lists to the Razor UI. `wwwroot/js/site.js` owns browser interaction, localStorage history, preferences, and calls `/api/translation/{provider}`.

## Coding Conventions

- Prefer the existing ASP.NET Core Options pattern: add configuration classes with `SectionName`, bind them in `Program.cs`, validate with data annotations when possible, and call `ValidateOnStart()` for required external service settings.
- Preserve cancellation-token flow from controllers into provider/service methods.
- Use structured logging placeholders instead of string interpolation in logs.
- Keep API DTOs in `Models/` and provider-specific options or clients in their provider folder.
- Keep route URLs lowercase and compatible with the configured lowercase route options.
- Do not store real Azure Translator, Foundry, OpenAI, or Docker credentials in source. Use environment variables, user secrets, or `appsettings.Development.json` for local values.
- Avoid broad refactors while changing provider behavior; maintain the existing small-service boundaries.

## Translation Provider Changes

When adding or changing a translation provider, update all affected surfaces:

- Register the client/service in `Program.cs` with a lifetime appropriate for its SDK usage.
- Add or update strongly typed options and configuration examples in `appsettings.json` only with placeholder values.
- Expose the provider through `IndexModel.ProviderList` if it should appear in the UI.
- Add or adjust the controller endpoint under `/api/translation/*` and keep request/response models compatible with `site.js`.
- Return `TranslationResult` with a stable `ProviderCode`; include detected-language metadata when the provider supplies it.
- Preserve rate limiting with the `TranslateLimiter` policy for translation endpoints.

## Frontend Conventions

- Use existing Fluent UI Web Components (`fluent-button`, `fluent-dropdown`, `fluent-textarea`, etc.) and the current CSS utility classes in `wwwroot/css/site.css`.
- Keep browser state local to `site.js` managers for history and preferences.
- Escape user-provided content before inserting HTML into history markup. Prefer `textContent` when not intentionally building markup.
- Keep the request payload shape aligned with `TranslationRequest`: `Content`, nullable `FromLang`, and `ToLang`.
- Keep the response handling aligned with ASP.NET JSON output, currently read as `translatedText`, `detectedLanguage`, and `confidence` when present.

## Build and Run

- Restore/build: `dotnet build Edi.Translator.slnx`
- Run locally: `dotnet run --project Edi.Translator.csproj`
- Docker build: `docker build -t editranslator .`

There is no test project in the current repository. For changes that affect provider selection, API behavior, validation, or error handling, prefer adding focused tests if a test project is introduced.

## Deployment Notes

- The Dockerfile publishes the web app from `mcr.microsoft.com/dotnet/sdk:10.0` into an ASP.NET runtime image and exposes ports `8080` and `8081`.
- The GitHub Actions workflow builds and pushes the Docker image from `master` using Docker Hub secrets.
- Azure App Service diagnostics logging is enabled only when `WEBSITE_SITE_NAME` is present.