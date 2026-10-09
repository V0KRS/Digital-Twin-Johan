using System.Text.Json;

namespace Dts.Domain.Projects;

/// <summary>
/// Project persistence boundary. First implementation is file-based and crash-safe; a PostgreSQL implementation
/// follows once NuGet/Postgres are reachable (docs/DECISIONS/0003). Every mutation creates a new revision.
/// </summary>
public interface IProjectStore
{
    Task<ProjectDocument> CreateAsync(string name, CancellationToken ct = default);
    Task<ProjectDocument> GetAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectSummary>> ListAsync(bool includeArchived, CancellationToken ct = default);
    /// <summary>Saves new content. Throws <see cref="RevisionConflictException"/> if expectedRevision is not the current one.</summary>
    Task<ProjectDocument> SaveAsync(string id, int expectedRevision, JsonElement content, CancellationToken ct = default);
    Task<ProjectDocument> RenameAsync(string id, string newName, CancellationToken ct = default);
    Task<ProjectDocument> DuplicateAsync(string id, string? newName, CancellationToken ct = default);
    Task<ProjectDocument> SetArchivedAsync(string id, bool archived, CancellationToken ct = default);
    Task<IReadOnlyList<RevisionInfo>> ListRevisionsAsync(string id, CancellationToken ct = default);
    Task<ProjectDocument> GetRevisionAsync(string id, int revision, CancellationToken ct = default);
}

public static class ProjectNames
{
    public const int MaxLength = 200;

    public static string Validate(string? name)
    {
        var n = name?.Trim();
        if (string.IsNullOrEmpty(n)) throw new InvalidProjectNameException("Project name is required.");
        if (n.Length > MaxLength) throw new InvalidProjectNameException($"Project name must be at most {MaxLength} characters.");
        if (n.Any(char.IsControl)) throw new InvalidProjectNameException("Project name must not contain control characters.");
        return n;
    }
}
