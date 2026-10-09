using System.Globalization;
using System.Text.Json;
using Dts.Domain;
using Dts.Domain.Projects;

namespace Dts.Host.Persistence;

/// <summary>
/// Crash-safe file store. Layout: {root}/projects/{id}/r{revision:D6}.json. Each file is a full immutable snapshot.
/// A save writes a temp file, flushes it to disk, then atomically renames it to its final revision name; a crash
/// at any point leaves earlier revisions untouched (leftover *.tmp files are ignored). The current state is the
/// highest revision that parses and validates. A corrupt newest file is kept on disk (evidence), skipped on read,
/// and logged; new revisions are numbered above it.
/// Limitation: the containing directory is not fsynced (not portable in .NET), so the very last rename could be
/// lost on power failure; the previous revision then remains the current one.
/// </summary>
public sealed class FileProjectStore : IProjectStore
{
    internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = false };

    private readonly string _projectsDir;
    private readonly TimeProvider _time;
    private readonly ILogger? _log;
    private readonly object _gate = new();

    /// <summary>Test seam: called with a stage name during save; may throw to simulate a crash.</summary>
    internal Action<string>? OnStage { get; init; }

    public FileProjectStore(string rootDir, TimeProvider? time = null, ILogger? log = null)
    {
        _projectsDir = Path.Combine(Path.GetFullPath(rootDir), "projects");
        _time = time ?? TimeProvider.System;
        _log = log;
        Directory.CreateDirectory(_projectsDir);
    }

    public Task<ProjectDocument> CreateAsync(string name, CancellationToken ct = default)
    {
        var n = ProjectNames.Validate(name);
        lock (_gate)
        {
            var now = _time.GetUtcNow();
            var doc = new ProjectDocument(Guid.NewGuid().ToString("N"), n, ProductInfo.ConfigSchemaVersion, 1, now, now, false, EmptyContent());
            Commit(doc);
            return Task.FromResult(doc);
        }
    }

    public Task<ProjectDocument> GetAsync(string id, CancellationToken ct = default)
    {
        lock (_gate) return Task.FromResult(LoadHead(id).Doc);
    }

    public Task<IReadOnlyList<ProjectSummary>> ListAsync(bool includeArchived, CancellationToken ct = default)
    {
        lock (_gate)
        {
            var list = new List<ProjectSummary>();
            foreach (var dir in Directory.EnumerateDirectories(_projectsDir))
            {
                var id = Path.GetFileName(dir);
                if (!IsValidId(id)) continue;
                try
                {
                    var d = LoadHead(id).Doc;
                    if (d.Archived && !includeArchived) continue;
                    list.Add(new ProjectSummary(d.Id, d.Name, d.Revision, d.UpdatedUtc, d.Archived, false));
                }
                catch (ProjectNotFoundException)
                {
                    _log?.LogError("Project {Id} has no readable revision.", id);
                    list.Add(new ProjectSummary(id, "(unreadable)", 0, null, false, true));
                }
            }
            return Task.FromResult<IReadOnlyList<ProjectSummary>>(
                list.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ThenBy(p => p.Id, StringComparer.Ordinal).ToList());
        }
    }

    public Task<ProjectDocument> SaveAsync(string id, int expectedRevision, JsonElement content, CancellationToken ct = default)
    {
        if (content.ValueKind != JsonValueKind.Object) throw new ArgumentException("Project content must be a JSON object.", nameof(content));
        lock (_gate)
        {
            var (head, maxNo) = LoadHead(id);
            if (head.Archived) throw new ProjectArchivedException(id);
            if (head.Revision != expectedRevision) throw new RevisionConflictException(id, expectedRevision, head.Revision);
            var next = head with { Revision = maxNo + 1, UpdatedUtc = _time.GetUtcNow(), Content = content.Clone() };
            Commit(next);
            return Task.FromResult(next);
        }
    }

    public Task<ProjectDocument> RenameAsync(string id, string newName, CancellationToken ct = default)
    {
        var n = ProjectNames.Validate(newName);
        lock (_gate)
        {
            var (head, maxNo) = LoadHead(id);
            if (head.Archived) throw new ProjectArchivedException(id);
            var next = head with { Name = n, Revision = maxNo + 1, UpdatedUtc = _time.GetUtcNow() };
            Commit(next);
            return Task.FromResult(next);
        }
    }

    public Task<ProjectDocument> DuplicateAsync(string id, string? newName, CancellationToken ct = default)
    {
        lock (_gate)
        {
            var src = LoadHead(id).Doc;
            var n = ProjectNames.Validate(string.IsNullOrWhiteSpace(newName) ? $"{src.Name} (copy)" : newName);
            var now = _time.GetUtcNow();
            var copy = new ProjectDocument(Guid.NewGuid().ToString("N"), n, src.SchemaVersion, 1, now, now, false, src.Content.Clone());
            Commit(copy);
            return Task.FromResult(copy);
        }
    }

    public Task<ProjectDocument> SetArchivedAsync(string id, bool archived, CancellationToken ct = default)
    {
        lock (_gate)
        {
            var (head, maxNo) = LoadHead(id);
            if (head.Archived == archived) return Task.FromResult(head);
            var next = head with { Archived = archived, Revision = maxNo + 1, UpdatedUtc = _time.GetUtcNow() };
            Commit(next);
            return Task.FromResult(next);
        }
    }

    public Task<IReadOnlyList<RevisionInfo>> ListRevisionsAsync(string id, CancellationToken ct = default)
    {
        lock (_gate)
        {
            var dir = ProjectDir(id);
            if (!Directory.Exists(dir)) throw new ProjectNotFoundException(id);
            var result = new List<RevisionInfo>();
            foreach (var no in RevisionNumbers(dir).OrderBy(x => x))
                if (TryRead(id, no) is { } d) result.Add(new RevisionInfo(d.Revision, d.UpdatedUtc, d.Name, d.Archived));
            return Task.FromResult<IReadOnlyList<RevisionInfo>>(result);
        }
    }

    public Task<ProjectDocument> GetRevisionAsync(string id, int revision, CancellationToken ct = default)
    {
        lock (_gate)
        {
            if (!Directory.Exists(ProjectDir(id))) throw new ProjectNotFoundException(id);
            return Task.FromResult(TryRead(id, revision) ?? throw new RevisionNotFoundException(id, revision));
        }
    }

    // ---- internals -------------------------------------------------------------------------------------------

    private static JsonElement EmptyContent() => JsonDocument.Parse("{}").RootElement.Clone();

    internal static bool IsValidId(string id) =>
        id.Length == 32 && id.All(c => c is (>= '0' and <= '9') or (>= 'a' and <= 'f'));

    private string ProjectDir(string id)
    {
        if (!IsValidId(id)) throw new ProjectNotFoundException(id); // also blocks path traversal
        return Path.Combine(_projectsDir, id);
    }

    private static string RevisionFile(string dir, int no) => Path.Combine(dir, $"r{no.ToString("D6", CultureInfo.InvariantCulture)}.json");

    private static IEnumerable<int> RevisionNumbers(string dir)
    {
        foreach (var f in Directory.EnumerateFiles(dir, "r??????.json"))
        {
            var name = Path.GetFileNameWithoutExtension(f);
            if (int.TryParse(name.AsSpan(1), NumberStyles.None, CultureInfo.InvariantCulture, out var n) && n > 0) yield return n;
        }
    }

    /// <summary>Highest readable revision plus the highest revision number present on disk (readable or not).</summary>
    private (ProjectDocument Doc, int MaxNo) LoadHead(string id)
    {
        var dir = ProjectDir(id);
        if (!Directory.Exists(dir)) throw new ProjectNotFoundException(id);
        var nums = RevisionNumbers(dir).OrderByDescending(x => x).ToList();
        foreach (var no in nums)
        {
            var d = TryRead(id, no);
            if (d is not null)
            {
                if (no != nums[0])
                    _log?.LogWarning("Project {Id}: newest revision r{Newest} is unreadable; serving r{No}.", id, nums[0], no);
                return (d, nums[0]);
            }
        }
        throw new ProjectNotFoundException(id);
    }

    private ProjectDocument? TryRead(string id, int no)
    {
        var path = RevisionFile(ProjectDir(id), no);
        if (!File.Exists(path)) return null;
        try
        {
            var d = JsonSerializer.Deserialize<ProjectDocument>(File.ReadAllBytes(path), Json);
            if (d is null || d.Id != id || d.Revision != no || d.Content.ValueKind != JsonValueKind.Object || string.IsNullOrWhiteSpace(d.Name))
                return null;
            return d;
        }
        catch (Exception e) when (e is JsonException or IOException or UnauthorizedAccessException)
        {
            _log?.LogError(e, "Project {Id}: revision r{No} is unreadable.", id, no);
            return null;
        }
    }

    private void Commit(ProjectDocument doc)
    {
        var dir = ProjectDir(doc.Id);
        Directory.CreateDirectory(dir);
        var final = RevisionFile(dir, doc.Revision);
        var tmp = Path.Combine(dir, $"{Guid.NewGuid():N}.tmp");
        try
        {
            using (var fs = new FileStream(tmp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                var bytes = JsonSerializer.SerializeToUtf8Bytes(doc, Json);
                OnStage?.Invoke("before-write");
                fs.Write(bytes, 0, bytes.Length / 2);   // staged so tests can crash between halves
                OnStage?.Invoke("mid-write");
                fs.Write(bytes, bytes.Length / 2, bytes.Length - bytes.Length / 2);
                fs.Flush(flushToDisk: true);
            }
            OnStage?.Invoke("before-rename");
            File.Move(tmp, final, overwrite: false);   // atomic; fails rather than clobber an existing revision
        }
        catch
        {
            try { if (File.Exists(tmp)) File.Delete(tmp); } catch { /* leftover .tmp is ignored by readers */ }
            throw;
        }
    }
}
