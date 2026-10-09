using System.Text.Json;
using Dts.Domain.Projects;

namespace Dts.Host;

public sealed record CreateProjectRequest(string? Name);
public sealed record SaveProjectRequest(int ExpectedRevision, JsonElement Content);
public sealed record RenameProjectRequest(string? Name);
public sealed record DuplicateProjectRequest(string? Name);

internal static class ProjectEndpoints
{
    public static void Map(WebApplication app)
    {
        var g = app.MapGroup("/api/projects");

        g.MapGet("/", async (bool? includeArchived, IProjectStore s, CancellationToken ct) =>
            Results.Ok(await s.ListAsync(includeArchived ?? false, ct)));

        g.MapPost("/", async (CreateProjectRequest r, IProjectStore s, CancellationToken ct) =>
        {
            var p = await s.CreateAsync(r.Name ?? "", ct);
            return Results.Created($"/api/projects/{p.Id}", p);
        });

        g.MapGet("/{id}", async (string id, IProjectStore s, CancellationToken ct) => Results.Ok(await s.GetAsync(id, ct)));

        g.MapPut("/{id}", async (string id, SaveProjectRequest r, IProjectStore s, CancellationToken ct) =>
            Results.Ok(await s.SaveAsync(id, r.ExpectedRevision, r.Content, ct)));

        g.MapPost("/{id}/rename", async (string id, RenameProjectRequest r, IProjectStore s, CancellationToken ct) =>
            Results.Ok(await s.RenameAsync(id, r.Name ?? "", ct)));

        g.MapPost("/{id}/duplicate", async (string id, DuplicateProjectRequest? r, IProjectStore s, CancellationToken ct) =>
        {
            var p = await s.DuplicateAsync(id, r?.Name, ct);
            return Results.Created($"/api/projects/{p.Id}", p);
        });

        g.MapPost("/{id}/archive", async (string id, IProjectStore s, CancellationToken ct) => Results.Ok(await s.SetArchivedAsync(id, true, ct)));
        g.MapPost("/{id}/unarchive", async (string id, IProjectStore s, CancellationToken ct) => Results.Ok(await s.SetArchivedAsync(id, false, ct)));

        g.MapGet("/{id}/revisions", async (string id, IProjectStore s, CancellationToken ct) => Results.Ok(await s.ListRevisionsAsync(id, ct)));
        g.MapGet("/{id}/revisions/{revision:int}", async (string id, int revision, IProjectStore s, CancellationToken ct) =>
            Results.Ok(await s.GetRevisionAsync(id, revision, ct)));
    }
}

/// <summary>Maps domain exceptions to problem-details responses. Unknown exceptions stay 500 and are not leaked.</summary>
internal sealed class ProjectExceptionMiddleware(RequestDelegate next, ILogger<ProjectExceptionMiddleware> log)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await next(ctx); }
        catch (Exception e) when (e is ProjectNotFoundException or RevisionNotFoundException or RevisionConflictException
                                      or ProjectArchivedException or ArgumentException or JsonException or BadHttpRequestException)
        {
            if (ctx.Response.HasStarted) throw;
            var (status, title) = e switch
            {
                ProjectNotFoundException or RevisionNotFoundException => (404, "Not found"),
                RevisionConflictException or ProjectArchivedException => (409, "Conflict"),
                _ => (400, "Bad request"),
            };
            log.LogInformation("Request rejected: {Status} {Message}", status, e.Message);
            ctx.Response.StatusCode = status;
            await ctx.Response.WriteAsJsonAsync(new { title, status, detail = e.Message });
        }
    }
}
