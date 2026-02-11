namespace Edi.Translator.Models;

public class TranslationResult
{
    public string ProviderCode { get; set; } = string.Empty;
    public string TranslatedText { get; set; } = string.Empty;
    public string? DetectedLanguage { get; set; }
    public float? Confidence { get; set; }
}
