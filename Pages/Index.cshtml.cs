using Microsoft.AspNetCore.Mvc.RazorPages;
using Edi.Translator.Models;

namespace Edi.Translator.Pages;

public class IndexModel : PageModel
{
    public const int MaxTextLength = 5000;

    public static readonly LanguageChoice[] LanguageList =
    [
        new LanguageChoice { Code = "auto-detect", Name = "Auto-Detect" },
        new LanguageChoice { Code = "zh-Hans", Name = "简体中文 (Simplified Chinese)" },
        new LanguageChoice { Code = "zh-Hant", Name = "繁體中文 (Traditional Chinese)" },
        new LanguageChoice { Code = "en-US", Name = "English (United States)" },
        new LanguageChoice { Code = "ar", Name = "??????? (Arabic)" },
        new LanguageChoice { Code = "de", Name = "Deutsch (German)" },
        new LanguageChoice { Code = "es", Name = "Espa?ol (Spanish)" },
        new LanguageChoice { Code = "fr", Name = "Fran?ais (French)" },
        new LanguageChoice { Code = "hi", Name = "?????? (Hindi)" },
        new LanguageChoice { Code = "id", Name = "Bahasa Indonesia (Indonesian)" },
        new LanguageChoice { Code = "it", Name = "Italiano (Italian)" },
        new LanguageChoice { Code = "ja", Name = "日本語 (Japanese)" },
        new LanguageChoice { Code = "ko", Name = "??? (Korean)" },
        new LanguageChoice { Code = "nl", Name = "Nederlands (Dutch)" },
        new LanguageChoice { Code = "pl", Name = "Polski (Polish)" },
        new LanguageChoice { Code = "pt", Name = "Português (Portuguese)" },
        new LanguageChoice { Code = "ru", Name = "Русский (Russian)" },
        new LanguageChoice { Code = "th", Name = "??? (Thai)" },
        new LanguageChoice { Code = "tr", Name = "Türk?e (Turkish)" },
        new LanguageChoice { Code = "vi", Name = "Ti?ng Vi?t (Vietnamese)" }
    ];

    public static readonly ApiProvider[] ProviderList =
    [
        new ApiProvider { Name = "Azure Translator", ApiRoute = "azure-translator" },
        new ApiProvider { Name = "GPT-4.1 (Azure)", ApiRoute = "aoai/gpt-4.1" },
        new ApiProvider { Name = "GPT-4.1-mini (Azure)", ApiRoute = "aoai/gpt-4.1-mini" },
        new ApiProvider { Name = "GPT-5-mini (Azure)", ApiRoute = "aoai/gpt-5-mini" }
    ];

    public void OnGet()
    {
    }
}
