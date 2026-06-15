using Edi.Translator.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Json;

namespace Edi.Translator.Security;

public sealed class MicrosoftAccountAuthenticationEvents(
    IAllowedEmailValidator allowedEmailValidator,
    IOptions<AppAuthenticationOptions> authenticationOptions,
    ILogger<MicrosoftAccountAuthenticationEvents> logger)
    : OAuthEvents
{
    public override Task CreatingTicket(OAuthCreatingTicketContext context)
    {
        var email = GetEmail(context);
        if (!allowedEmailValidator.IsAllowed(email))
        {
            logger.LogWarning("Rejected Microsoft account login for email: {Email}", email ?? "(missing)");
            context.Fail("This Microsoft account is not allowed to access this application.");
            return Task.CompletedTask;
        }

        if (context.Identity is not null &&
            !context.Identity.HasClaim(claim => claim.Type == ClaimTypes.Email))
        {
            context.Identity.AddClaim(new Claim(ClaimTypes.Email, email));
        }

        logger.LogInformation("Accepted Microsoft account login for email: {Email}", email);
        return Task.CompletedTask;
    }

    public override Task RemoteFailure(RemoteFailureContext context)
    {
        logger.LogWarning(context.Failure, "Microsoft account login failed");

        context.HandleResponse();
        context.Response.Redirect(authenticationOptions.Value.AccessDeniedPath);
        return Task.CompletedTask;
    }

    private static string GetEmail(OAuthCreatingTicketContext context)
    {
        var email = context.Identity?.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrWhiteSpace(email))
        {
            return email;
        }

        foreach (var propertyName in new[] { "mail", "userPrincipalName", "email" })
        {
            if (context.User.TryGetProperty(propertyName, out var value) &&
                value.ValueKind == JsonValueKind.String)
            {
                email = value.GetString();
                if (!string.IsNullOrWhiteSpace(email))
                {
                    return email;
                }
            }
        }

        return null;
    }
}
