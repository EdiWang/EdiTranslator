using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Edi.Translator.Models;
using Edi.Translator.Providers.AzureOpenAI;

namespace Edi.Translator.Pages;

public class IndexModel(IOptions<AzureOpenAIOptions> openAIOptions) : PageModel
{
    private readonly AzureOpenAIOptions _openAIOptions = openAIOptions.Value;

    public const int MaxTextLength = 5000;

    public static readonly LanguageChoice[] LanguageList =
    [
        new LanguageChoice { Code = "auto-detect", Name = "Auto-Detect" },
        new LanguageChoice { Code = "zh-Hans", Name = "简体中文 (Simplified Chinese)" },
        new LanguageChoice { Code = "zh-Hant", Name = "繁體中文 (Traditional Chinese)" },
        new LanguageChoice { Code = "en-US", Name = "English (United States)" },
        new LanguageChoice { Code = "ar", Name = "العربية (Arabic)" },
        new LanguageChoice { Code = "de", Name = "Deutsch (German)" },
        new LanguageChoice { Code = "es", Name = "Español (Spanish)" },
        new LanguageChoice { Code = "fr", Name = "Français (French)" },
        new LanguageChoice { Code = "hi", Name = "हिन्दी (Hindi)" },
        new LanguageChoice { Code = "id", Name = "Bahasa Indonesia (Indonesian)" },
        new LanguageChoice { Code = "it", Name = "Italiano (Italian)" },
        new LanguageChoice { Code = "ja", Name = "日本語 (Japanese)" },
        new LanguageChoice { Code = "ko", Name = "한국어 (Korean)" },
        new LanguageChoice { Code = "nl", Name = "Nederlands (Dutch)" },
        new LanguageChoice { Code = "pl", Name = "Polski (Polish)" },
        new LanguageChoice { Code = "pt", Name = "Português (Portuguese)" },
        new LanguageChoice { Code = "ru", Name = "Русский (Russian)" },
        new LanguageChoice { Code = "th", Name = "ไทย (Thai)" },
        new LanguageChoice { Code = "tr", Name = "Türkçe (Turkish)" },
        new LanguageChoice { Code = "vi", Name = "Tiếng Việt (Vietnamese)" }
    ];

    public ApiProvider[] ProviderList
    {
        get
        {
            var providers = new List<ApiProvider>
            {
                new ApiProvider { Name = "Azure Translator", ApiRoute = "azure-translator" }
            };

            if (_openAIOptions.Deployments != null)
            {
                providers.AddRange(_openAIOptions.Deployments.Select(d =>
                    new ApiProvider
                    {
                        Name = d.DisplayName,
                        ApiRoute = $"ai/{d.Name}"
                    }));
            }

            return providers.ToArray();
        }
    }

    public void OnGet()
    {
    }
}
