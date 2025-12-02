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

public class TranslationHistory
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public LanguageChoice SourceLanguage { get; set; } = new();
    public LanguageChoice TargetLanguage { get; set; } = new();
    public string SourceText { get; set; } = string.Empty;
    public string TranslatedText { get; set; } = string.Empty;
    public ApiProvider Provider { get; set; } = new();
}
