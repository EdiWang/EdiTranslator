﻿namespace Edi.AzureTranslatorProxy.Auth;

public class ApiKey
{
    public string Owner { get; set; }
    public string Key { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; } = new[] { "User" };
}