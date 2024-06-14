namespace Edi.Translator.Auth;

public interface IGetApiKeyQuery
{
    Task<ApiKey> Execute(string providedApiKey);
}