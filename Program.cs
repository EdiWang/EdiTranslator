using Edi.Translator.Configuration;
using Edi.Translator.Models;
using Edi.Translator.Providers.AzureOpenAI;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Edi.Translator;

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
        builder.Services.AddRazorPages();
        builder.Services.AddHttpClient();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddApplicationInsightsTelemetry();
        builder.Services.AddScoped<IAOAIClient, AOAIClient>();

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

        builder.Services.Configure<AzureTranslatorConfig>(
            builder.Configuration.GetSection(AzureTranslatorConfig.SectionName));

        builder.Services.AddOptions<AzureTranslatorConfig>()
            .BindConfiguration(AzureTranslatorConfig.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        builder.Services.AddOptions<AzureOpenAIOptions>()
            .BindConfiguration(AzureOpenAIOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

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
        app.UseAuthorization();
        app.UseRateLimiter();

        app.MapRazorPages();
        app.MapControllers();

        app.Run();
    }
}