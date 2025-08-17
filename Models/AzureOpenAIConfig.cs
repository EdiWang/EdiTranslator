using System.ComponentModel.DataAnnotations;

namespace Edi.Translator.Models;

public class AzureOpenAIConfig
{
    public const string SectionName = "AzureOpenAI";

    [Required]
    public string[] DeploymentNames { get; set; } = Array.Empty<string>();
}