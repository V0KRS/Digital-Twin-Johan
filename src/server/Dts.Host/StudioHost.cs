using Dts.Domain;
using Dts.Domain.Projects;
using Dts.Host.Persistence;

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

        // Project persistence: file-based store under Dts:DataDir (default: per-user local app data).
        var dataDir = builder.Configuration["Dts:DataDir"];
        if (string.IsNullOrWhiteSpace(dataDir))
            dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DigitalTwinStudio");
        builder.Services.AddSingleton<IProjectStore>(sp =>
            new FileProjectStore(dataDir, TimeProvider.System, sp.GetRequiredService<ILoggerFactory>().CreateLogger("Dts.ProjectStore")));

        var app = builder.Build();
        app.UseMiddleware<ProjectExceptionMiddleware>();
        MapEndpoints(app);
        return app;
    }

    internal static void MapEndpoints(WebApplication app)
    {
        ProjectEndpoints.Map(app);
        app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
        app.MapGet("/api/version", () => Results.Ok(new
        {
            product = ProductInfo.Name,
            version = ProductInfo.Version,
            configSchemaVersion = ProductInfo.ConfigSchemaVersion,
        }));
    }
}
