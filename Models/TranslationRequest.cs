using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Edi.Translator.Models;

public class TranslationRequest
{
    [Required]
    public string Content { get; set; }

    [Required]
    public string FromLang { get; set; }

    [Required]
    public string ToLang { get; set; }
}

public class AOAIMessage
{
    public string Role { get; set; }

    public string Content { get; set; }
}

public class AOAIRequest
{
    public List<AOAIMessage> Messages { get; set; }
}