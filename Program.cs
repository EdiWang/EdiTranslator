using Edi.Translator.Configuration;
using Edi.Translator.Providers.MicrosoftFoundry;
using Edi.Translator.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;

namespace Edi.Translator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        if (EnvironmentHelper.IsRunningOnAzureAppService())
        {
            builder.Logging.AddAzureWebAppDiagnostics();
        }

        builder.Services.AddControllers();
        builder.Services.AddRazorPages();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddScoped<IFoundryClient, FoundryClient>();
        builder.Services.AddScoped<IAllowedEmailValidator, AllowedEmailValidator>();
        builder.Services.AddScoped<MicrosoftAccountAuthenticationEvents>();
        builder.Services.AddSingleton<IValidateOptions<AppAuthenticationOptions>, AppAuthenticationOptionsValidator>();

        builder.Services.AddOptions<AppAuthenticationOptions>()
            .BindConfiguration(AppAuthenticationOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var authenticationOptions = builder.Configuration
            .GetSection(AppAuthenticationOptions.SectionName)
            .Get<AppAuthenticationOptions>() ?? new AppAuthenticationOptions();

        if (authenticationOptions.Enabled)
        {
            var microsoftAuthenticationOptions = authenticationOptions.Providers?.Microsoft ?? new MicrosoftAccountProviderOptions();

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.Cookie.Name = ".EdiTranslator.Auth";
                    options.LoginPath = authenticationOptions.LoginPath;
                    options.AccessDeniedPath = authenticationOptions.AccessDeniedPath;
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToLogin = context =>
                        {
                            if (context.Request.Path.StartsWithSegments("/api"))
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                return Task.CompletedTask;
                            }

                            context.Response.Redirect(context.RedirectUri);
                            return Task.CompletedTask;
                        },
                        OnRedirectToAccessDenied = context =>
                        {
                            if (context.Request.Path.StartsWithSegments("/api"))
                            {
                                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                return Task.CompletedTask;
                            }

                            context.Response.Redirect(context.RedirectUri);
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddMicrosoftAccount(options =>
                {
                    options.ClientId = microsoftAuthenticationOptions.ClientId;
                    options.ClientSecret = microsoftAuthenticationOptions.ClientSecret;
                    options.CallbackPath = microsoftAuthenticationOptions.CallbackPath;
                    options.AuthorizationEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";
                    options.TokenEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
                    options.EventsType = typeof(MicrosoftAccountAuthenticationEvents);
                });
        }

        builder.Services.AddAuthorization(options =>
        {
            if (authenticationOptions.Enabled)
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            }
        });

        builder.Services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
            options.AppendTrailingSlash = false;
        });

        builder.Services.AddRateLimiter(limiterOptions =>
        {
            limiterOptions.OnRejected = async (context, ct) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync("Too Many Requests", ct);
            };

            void AddLimiter(string policyName, int eventCount, TimeSpan perTimeSpan)
            {
                limiterOptions.AddFixedWindowLimiter(
                    policyName: policyName,
                    RateLimiterOptionFactory.GetFixedWindowRateLimiterOptions(eventCount, perTimeSpan));
            }

            AddLimiter("TranslateLimiter", 5, TimeSpan.FromSeconds(1));
        });

        builder.Services.AddOptions<MicrosoftFoundryOptions>()
            .BindConfiguration(MicrosoftFoundryOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.UseStaticFiles(new StaticFileOptions()
        {
            OnPrepareResponse = context =>
            {
                context.Context.Response.Headers.TryAdd("Cache-Control", "no-cache, no-store");
                context.Context.Response.Headers.TryAdd("Expires", "-1");
            }
        });

        app.UseRouting();
        if (authenticationOptions.Enabled)
        {
            app.UseAuthentication();
        }

        app.UseAuthorization();
        app.UseRateLimiter();

        app.MapRazorPages();
        app.MapControllers();

        app.Run();
    }
}
