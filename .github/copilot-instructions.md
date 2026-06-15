# Project Guidelines

## Project Shape

This is a .NET 10 ASP.NET Core web app for text translation. It serves a Razor Pages UI and Web API endpoints from the same project.

- Keep server code under the existing namespaces rooted at `Edi.Translator`.
- The UI is Razor Pages plus plain JavaScript and Fluent UI Web Components loaded from CDN; do not introduce a SPA framework or frontend build pipeline unless explicitly requested.
- The app has optional built-in SSO authentication controlled by `Authentication:Enabled`. Do not assume users are authenticated unless that setting is enabled; deployment can still place SSO or auth in front of the app.

## Architecture

- `Program.cs` wires services, strongly typed options, route casing, static files, rate limiting, Razor Pages, and controllers.
- `Controllers/TranslationController.cs` owns HTTP API behavior under `/api/translation/*` and should stay thin: validate input, select deployment, call the Foundry client, map exceptions to HTTP responses, and log with structured properties.
- `Providers/MicrosoftFoundry/FoundryClient.cs` wraps Azure OpenAI/Microsoft Foundry chat completions. Preserve the system prompt rules when adjusting AI translation behavior.
- `Pages/Index.cshtml.cs` provides language and Microsoft Foundry deployment lists to the Razor UI. `wwwroot/js/site.js` owns browser interaction, localStorage history, preferences, and calls `/api/translation/{deploymentName}/stream`.
- `Pages/Account/*` owns the built-in login, logout, and access-denied Razor Pages. `Security/*` owns built-in authentication helpers and Microsoft account ticket validation.

## Coding Conventions

- Prefer the existing ASP.NET Core Options pattern: add configuration classes with `SectionName`, bind them in `Program.cs`, validate with data annotations when possible, and call `ValidateOnStart()` for required external service settings.
- Preserve cancellation-token flow from controllers into Foundry client methods.
- Use structured logging placeholders instead of string interpolation in logs.
- Keep API DTOs in `Models/` and Foundry-specific options or clients in `Providers/MicrosoftFoundry/`.
- Keep route URLs lowercase and compatible with the configured lowercase route options.
- Do not store real Foundry, OpenAI, or Docker credentials in source. Use environment variables, user secrets, or `appsettings.Development.json` for local values.
- Keep built-in authentication optional and controlled by `Authentication:Enabled`.
- Keep authentication allow-list checks based on `Authentication:AllowedEmails`; do not introduce a database for login state unless explicitly requested.
- Avoid broad refactors while changing Foundry behavior; maintain the existing small-service boundaries.

## Microsoft Foundry Translation Changes

Microsoft Foundry is the only translation backend. When changing Foundry behavior or deployment selection, update all affected surfaces:

- Register the client in `Program.cs` with a lifetime appropriate for its SDK usage.
- Add or update strongly typed options and configuration examples in `appsettings.json` only with placeholder values.
- Expose enabled deployments through `IndexModel.DeploymentList` if they should appear in the UI.
- Add or adjust the controller endpoint under `/api/translation/*` and keep request/response models compatible with `site.js`.
- Return `TranslationResult` with the selected deployment name.
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

There is no test project in the current repository. For changes that affect deployment selection, API behavior, validation, or error handling, prefer adding focused tests if a test project is introduced.

## Deployment Notes

- The Dockerfile publishes the web app from `mcr.microsoft.com/dotnet/sdk:10.0` into an ASP.NET runtime image and exposes ports `8080` and `8081`.
- The GitHub Actions workflow builds and pushes the Docker image from `master` using Docker Hub secrets.
- Azure App Service diagnostics logging is enabled only when `WEBSITE_SITE_NAME` is present.
