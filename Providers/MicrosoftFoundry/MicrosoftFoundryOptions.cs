namespace Edi.Translator.Providers.MicrosoftFoundry;

public class MicrosoftFoundryOptions
{
    public const string SectionName = "MicrosoftFoundry";

    public required string Endpoint { get; init; }
    public required string Key { get; init; }
    public MicrosoftFoundryDeploymentOption[] Deployments { get; init; } = [];
}

public class MicrosoftFoundryDeploymentOption
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public bool Enabled { get; init; } = true;
}
