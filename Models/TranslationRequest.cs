using System.ComponentModel.DataAnnotations;

namespace Edi.AzureTranslatorProxy.Models;

public class TranslationRequest
{
    [Required]
    public string Content { get; set; }

    [Required]
    public string FromLang { get; set; }

    [Required]
    public string ToLang { get; set; }
}