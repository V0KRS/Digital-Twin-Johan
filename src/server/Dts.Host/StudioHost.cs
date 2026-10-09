using Dts.Domain;

namespace Dts.Host;

public static class StudioHost
{
    /// <summary>Builds the web application. Separate from Program so tests can start it in-process.</summary>
    public static WebApplication Build(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Structured (JSON) console logging; no credentials are ever logged by this layer.
        builder.Logging.ClearProviders();
        builder.Logging.AddJsonConsole(o => o.IncludeScopes = true);

        // Secure default: loopback only. Network exposure must be an explicit config choice.
        if (builder.Configuration["urls"] is null && builder.Configuration["ASPNETCORE_URLS"] is null)
            builder.WebHost.UseUrls("http://127.0.0.1:5080");

        builder.Services.AddHealthChecks();

        var app = builder.Build();
        MapEndpoints(app);
        return app;
    }

    internal static void MapEndpoints(WebApplication app)
    {
        app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
        app.MapGet("/api/version", () => Results.Ok(new
        {
            product = ProductInfo.Name,
            version = ProductInfo.Version,
            configSchemaVersion = ProductInfo.ConfigSchemaVersion,
        }));
    }
}
