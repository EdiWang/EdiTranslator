using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Edi.Translator.Pages.Account;

[AllowAnonymous]
public class DeniedModel : PageModel
{
    public void OnGet()
    {
    }
}
