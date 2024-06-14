namespace Edi.AzureTranslatorProxy.Models;

public class TranslationResponse
{
    public List<Translation> Translations { get; set; }
}

public class Translation
{
    public string Text { get; set; }
    public string To { get; set; }
}