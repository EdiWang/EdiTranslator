# AGENTS.md

Guidance for Codex and other coding agents working in this repository.

## Project Overview

Edi's Translator is a .NET 10 ASP.NET Core web app for text translation. It serves a Razor Pages UI and Web API endpoints from the same project.

- Keep server code under namespaces rooted at `Edi.Translator`.
- The UI is Razor Pages plus plain JavaScript and Fluent UI Web Components loaded from CDN.
- Do not introduce a SPA framework, frontend build pipeline, or large architectural rewrite unless explicitly requested.
- The app has no built-in authentication. Do not assume users are authenticated; deployments may put SSO or another auth layer in front of the app.

## Repository Map

- `Program.cs` wires services, options, route casing, static files, rate limiting, Razor Pages, and controllers.
- `Controllers/TranslationController.cs` owns HTTP API behavior under `/api/translation/*`.
- `Providers/MicrosoftFoundry/FoundryClient.cs` wraps Azure OpenAI/Microsoft Foundry chat completions.
- `Pages/Index.cshtml.cs` prepares language and Microsoft Foundry deployment lists for the Razor UI.
- `Pages/Index.cshtml` renders the main translation page.
- `wwwroot/js/site.js` owns browser interaction, localStorage history, preferences, and calls `/api/translation/{deploymentName}/stream`.
- `wwwroot/css/site.css` contains the app styling.
- `.github/copilot-instructions.md` contains the older Copilot-oriented guidance and should stay in sync with this file when project conventions change.

## Codex Working Style

- Start by reading the files directly related to the requested change before editing.
- Prefer small, targeted changes that preserve existing service boundaries.
- Do not revert user changes or unrelated working-tree changes.
- Use `rg` or `rg --files` for searches.
- Use `dotnet build Edi.Translator.slnx` as the default verification command after code changes.
- If a change affects browser behavior, run the app with `dotnet run --project Edi.Translator.csproj` and verify the relevant UI/API path when practical.
- If verification cannot be run because credentials or external services are missing, state exactly what was and was not verified.

## Coding Conventions

- Use the existing ASP.NET Core Options pattern:
  - Add configuration classes with `SectionName`.
  - Bind them in `Program.cs`.
  - Validate with data annotations where possible.
  - Use `ValidateOnStart()` for required external service settings.
- Preserve cancellation-token flow from controllers into Foundry client methods.
- Use structured logging placeholders instead of string interpolation in logs.
- Keep API DTOs in `Models/`.
- Keep Foundry-specific options and clients in the `Providers/MicrosoftFoundry/` folder.
- Keep route URLs lowercase and compatible with the configured lowercase route options.
- Do not store real Microsoft Foundry, OpenAI, Docker, or other service credentials in source.
- Use placeholder values in committed configuration examples.
- Avoid broad refactors while changing Foundry behavior.

## Microsoft Foundry Translation Changes

Microsoft Foundry is the only translation backend. When changing Foundry behavior or deployment selection, update every affected surface:

- Register the client in `Program.cs` with a lifetime appropriate for its SDK usage.
- Add or update strongly typed options.
- Update `appsettings.json` only with placeholder values.
- Expose enabled deployments through `IndexModel.DeploymentList` if they should appear in the UI.
- Add or adjust controller endpoints under `/api/translation/*`.
- Keep request/response models compatible with `wwwroot/js/site.js`.
- Return `TranslationResult` with the selected deployment name.
- Preserve rate limiting with the `TranslateLimiter` policy for translation endpoints.
- Preserve the Microsoft Foundry system prompt rules when changing AI translation behavior.

## API Behavior

- Keep `TranslationController` thin:
  - Validate input.
  - Select deployment.
  - Call the Foundry client.
  - Map expected Foundry exceptions to appropriate HTTP responses.
  - Log failures with structured properties.
- Keep the request payload shape aligned with `TranslationRequest`:
  - `Content`
  - nullable `FromLang`
  - `ToLang`
- Keep JSON response handling aligned with the frontend, currently `translatedText`, `detectedLanguage`, and `confidence` when present.

## Frontend Conventions

- Use existing Fluent UI Web Components such as `fluent-button`, `fluent-dropdown`, `fluent-option`, and `fluent-textarea`.
- Keep browser state local to the existing managers in `wwwroot/js/site.js`.
- Escape user-provided content before inserting HTML into history markup.
- Prefer `textContent` when intentionally building markup is not required.
- Do not add npm, bundlers, TypeScript, React, Vue, Angular, or similar frontend tooling unless explicitly requested.
- Keep UI changes consistent with the current Razor Pages structure and CSS classes.

## Build, Run, and Docker

Use these commands from the repository root:

```powershell
dotnet build Edi.Translator.slnx
dotnet run --project Edi.Translator.csproj
docker build -t editranslator .
```

The Dockerfile publishes the web app from `mcr.microsoft.com/dotnet/sdk:10.0` into an ASP.NET runtime image and exposes ports `8080` and `8081`.

## Tests and Verification

There is currently no test project in the repository.

- For narrow documentation or configuration-only changes, a build may not be necessary.
- For C# changes, run `dotnet build Edi.Translator.slnx`.
- For deployment selection, API validation, error handling, or request/response contract changes, prefer adding focused tests if a test project is introduced.
- For frontend behavior changes, verify the affected workflow in a browser when practical.

## Deployment Notes

- The GitHub Actions workflow builds and pushes the Docker image from `master` using Docker Hub secrets.
- Azure App Service diagnostics logging is enabled only when `WEBSITE_SITE_NAME` is present.
- Keep deployment documentation and Docker examples free of real secrets.
