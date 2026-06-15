using System.ComponentModel.DataAnnotations;

namespace Edi.Translator.Configuration;

public sealed class AppAuthenticationOptions
{
    public const string SectionName = "Authentication";

    public bool Enabled { get; set; }

    [Required]
    public string LoginPath { get; set; } = "/account/login";

    [Required]
    public string AccessDeniedPath { get; set; } = "/account/denied";

    public List<string> AllowedEmails { get; set; } = [];

    public AuthenticationProviderOptions Providers { get; set; } = new();
}

public sealed class AuthenticationProviderOptions
{
    public MicrosoftAccountProviderOptions Microsoft { get; set; } = new();
}

public sealed class MicrosoftAccountProviderOptions
{
    public bool Enabled { get; set; } = true;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    [Required]
    public string CallbackPath { get; set; } = "/signin-microsoft";
}
