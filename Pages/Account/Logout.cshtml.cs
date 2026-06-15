using Edi.Translator.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Edi.Translator.Pages.Account;

[AllowAnonymous]
public class LogoutModel(IOptions<AppAuthenticationOptions> authenticationOptions) : PageModel
{
    public IActionResult OnGet()
    {
        return RedirectToPage("/Index");
    }

    public IActionResult OnPost()
    {
        if (!authenticationOptions.Value.Enabled)
        {
            return RedirectToPage("/Index");
        }

        return SignOut(
            new AuthenticationProperties { RedirectUri = Url.Page("/Account/Login") ?? "/" },
            CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
