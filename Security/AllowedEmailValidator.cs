using Edi.Translator.Configuration;
using Microsoft.Extensions.Options;

namespace Edi.Translator.Security;

public interface IAllowedEmailValidator
{
    bool IsAllowed(string email);
}

public sealed class AllowedEmailValidator(IOptions<AppAuthenticationOptions> options) : IAllowedEmailValidator
{
    public bool IsAllowed(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return options.Value.AllowedEmails?.Any(allowedEmail =>
            string.Equals(allowedEmail.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase)) == true;
    }
}
