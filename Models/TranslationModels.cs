namespace Edi.Translator.Models;

public class LanguageChoice
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class ApiProvider
{
    public string Name { get; set; } = string.Empty;
    public string ApiRoute { get; set; } = string.Empty;
}
