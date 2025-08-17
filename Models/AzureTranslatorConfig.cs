using System.ComponentModel.DataAnnotations;

namespace Edi.Translator.Models;

// Configuration classes for strongly-typed options
public class AzureTranslatorConfig
{
    public const string SectionName = "AzureTranslator";

    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Region { get; set; } = string.Empty;
}
