using System.Text.Json;

namespace Dts.Domain.Projects;

/// <summary>One immutable saved revision of a project. Content is opaque JSON owned by later increments (scene etc.).</summary>
public sealed record ProjectDocument(
    string Id,
    string Name,
    int SchemaVersion,
    int Revision,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc,
    bool Archived,
    JsonElement Content);

/// <summary>List entry. <see cref="Damaged"/> means no readable revision exists; it is never hidden from the list.</summary>
public sealed record ProjectSummary(string Id, string Name, int Revision, DateTimeOffset? UpdatedUtc, bool Archived, bool Damaged);

public sealed record RevisionInfo(int Revision, DateTimeOffset UpdatedUtc, string Name, bool Archived);

public class ProjectNotFoundException(string id) : Exception($"Project '{id}' was not found.");
public class RevisionNotFoundException(string id, int revision) : Exception($"Revision {revision} of project '{id}' was not found.");
public class RevisionConflictException(string id, int expected, int actual)
    : Exception($"Project '{id}' is at revision {actual}, but revision {expected} was expected. Reload before saving.")
{
    public int Expected { get; } = expected;
    public int Actual { get; } = actual;
}
public class ProjectArchivedException(string id) : Exception($"Project '{id}' is archived and read-only. Unarchive it first.");
public class InvalidProjectNameException(string message) : ArgumentException(message);
