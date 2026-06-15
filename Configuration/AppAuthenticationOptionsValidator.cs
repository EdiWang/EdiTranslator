using Microsoft.Extensions.Options;

namespace Edi.Translator.Configuration;

public sealed class AppAuthenticationOptionsValidator : IValidateOptions<AppAuthenticationOptions>
{
    public ValidateOptionsResult Validate(string name, AppAuthenticationOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        List<string> failures = [];

        if (options.AllowedEmails is null ||
            options.AllowedEmails.Count == 0 ||
            options.AllowedEmails.All(string.IsNullOrWhiteSpace))
        {
            failures.Add("Authentication:AllowedEmails must contain at least one email address when authentication is enabled.");
        }

        if (string.IsNullOrWhiteSpace(options.LoginPath) || !options.LoginPath.StartsWith('/'))
        {
            failures.Add("Authentication:LoginPath must start with '/'.");
        }

        if (string.IsNullOrWhiteSpace(options.AccessDeniedPath) || !options.AccessDeniedPath.StartsWith('/'))
        {
            failures.Add("Authentication:AccessDeniedPath must start with '/'.");
        }

        var microsoft = options.Providers?.Microsoft;
        if (microsoft is null || !microsoft.Enabled)
        {
            failures.Add("Authentication:Providers:Microsoft:Enabled must be true when authentication is enabled in phase one.");
        }

        if (string.IsNullOrWhiteSpace(microsoft?.ClientId))
        {
            failures.Add("Authentication:Providers:Microsoft:ClientId is required when authentication is enabled.");
        }

        if (string.IsNullOrWhiteSpace(microsoft?.ClientSecret))
        {
            failures.Add("Authentication:Providers:Microsoft:ClientSecret is required when authentication is enabled.");
        }

        if (string.IsNullOrWhiteSpace(microsoft?.CallbackPath) || !microsoft.CallbackPath.StartsWith('/'))
        {
            failures.Add("Authentication:Providers:Microsoft:CallbackPath must start with '/'.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
