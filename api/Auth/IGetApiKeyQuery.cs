namespace Edi.AzureTranslatorProxy.Auth;

public interface IGetApiKeyQuery
{
    Task<ApiKey> Execute(string providedApiKey);
}