using Microsoft.AspNetCore.Authentication;

namespace Edi.Translator.Auth;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "APIKey";
    public static string Scheme => DefaultScheme;
    public string AuthenticationType = DefaultScheme;
}