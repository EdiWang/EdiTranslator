namespace Edi.Translator;

public static class EnvironmentHelper
{
    public static bool IsRunningOnAzureAppService()
    {
        return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
    }
}