using Edi.AzureTranslatorProxy.Auth;
using Edi.AzureTranslatorProxy.Configuration;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Edi.AzureTranslatorProxy;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        if (Helper.IsRunningOnAzureAppService())
        {
            builder.Logging.AddAzureWebAppDiagnostics();
        }

        builder.Services.AddControllers();
        builder.Services.AddHttpClient();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddApplicationInsightsTelemetry();

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

        if (bool.Parse(builder.Configuration["EnableApiKeyAuthentication"]!))
        {
            builder.Services.Configure<List<ApiKey>>(builder.Configuration.GetSection("ApiKeys"));
            builder.Services.AddScoped<IGetApiKeyQuery, AppSettingsGetApiKeyQuery>();
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
                    options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
                })
                .AddApiKeySupport(options => { });
        }

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.UseRateLimiter();

        app.Run();
    }
}