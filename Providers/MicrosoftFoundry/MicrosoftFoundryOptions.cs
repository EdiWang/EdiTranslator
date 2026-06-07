using System.ComponentModel.DataAnnotations;

namespace Edi.Translator.Providers.MicrosoftFoundry;

public class MicrosoftFoundryOptions
{
    public const string SectionName = "MicrosoftFoundry";

    [Required]
    public required string Endpoint { get; init; }

    [Required]
    public required string Key { get; init; }

    [Required]
    [MinLength(1)]
    public MicrosoftFoundryDeploymentOption[] Deployments { get; init; } = [];
}

public class MicrosoftFoundryDeploymentOption
{
    [Required]
    public required string Name { get; init; }

    [Required]
    public required string DisplayName { get; init; }

    public bool Enabled { get; init; } = true;
}
