using Edi.Translator.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Edi.Translator.Pages.Account;

[AllowAnonymous]
public class LoginModel(IOptions<AppAuthenticationOptions> authenticationOptions) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string ReturnUrl { get; set; }

    public IActionResult OnGet()
    {
        if (!authenticationOptions.Value.Enabled)
        {
            return RedirectToPage("/Index");
        }

        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(GetLocalReturnUrl(ReturnUrl));
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!authenticationOptions.Value.Enabled)
        {
            return RedirectToPage("/Index");
        }

        var properties = new AuthenticationProperties
        {
            RedirectUri = GetLocalReturnUrl(ReturnUrl)
        };

        return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
    }

    private string GetLocalReturnUrl(string returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return returnUrl;
        }

        return Url.Page("/Index") ?? "/";
    }
}
