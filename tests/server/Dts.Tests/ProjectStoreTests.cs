using System.Text.Json;
using Dts.Domain.Projects;
using Dts.Host.Persistence;

namespace Dts.Tests;

public class ProjectStoreTests
{
    internal static string TempDir() => Path.Combine(Path.GetTempPath(), "dts-test-" + Guid.NewGuid().ToString("N"));
    internal static JsonElement J(string json) => JsonDocument.Parse(json).RootElement.Clone();
    private static string Norm(JsonElement e) => JsonSerializer.Serialize(e);
    private static string[] Files(string root, string id) => Directory.GetFiles(Path.Combine(root, "projects", id));

    [Test]
    public async Task Create_then_get_returns_revision_1()
    {
        var s = new FileProjectStore(TempDir());
        var p = await s.CreateAsync("  Pump Station A  ");
        Assert.Equal("Pump Station A", p.Name);
        Assert.Equal(1, p.Revision);
        var g = await s.GetAsync(p.Id);
        Assert.Equal(p.Id, g.Id);
        Assert.Equal(1, g.SchemaVersion);
    }

    [Test]
    public async Task Invalid_names_are_rejected()
    {
        var s = new FileProjectStore(TempDir());
        await Assert.ThrowsAsync<InvalidProjectNameException>(() => s.CreateAsync("   "));
        await Assert.ThrowsAsync<InvalidProjectNameException>(() => s.CreateAsync(new string('x', 201)));
        await Assert.ThrowsAsync<InvalidProjectNameException>(() => s.CreateAsync("bad\nname"));
    }

    [Test]
    public async Task Path_traversal_ids_are_not_found()
    {
        var s = new FileProjectStore(TempDir());
        await Assert.ThrowsAsync<ProjectNotFoundException>(() => s.GetAsync("../../etc"));
        await Assert.ThrowsAsync<ProjectNotFoundException>(() => s.GetAsync(new string('a', 32)));
    }

    [Test]
    public async Task Save_reload_after_restart_is_identical()
    {
        var root = TempDir();
        var s1 = new FileProjectStore(root);
        var p = await s1.CreateAsync("Plant");
        var content = J("""{"objects":[{"id":"o1","type":"box","position":[1,2,3]}]}""");
        var saved = await s1.SaveAsync(p.Id, p.Revision, content);
        Assert.Equal(2, saved.Revision);

        var s2 = new FileProjectStore(root); // "restart": brand-new instance, no shared memory
        var loaded = await s2.GetAsync(p.Id);
        Assert.Equal(Norm(content), Norm(loaded.Content));
        Assert.Equal(2, loaded.Revision);
        Assert.Equal("Plant", loaded.Name);
    }

    [Test]
    public async Task Stale_revision_save_is_a_conflict_and_changes_nothing()
    {
        var s = new FileProjectStore(TempDir());
        var p = await s.CreateAsync("P");
        await s.SaveAsync(p.Id, 1, J("""{"a":1}"""));
        var e = await Assert.ThrowsAsync<RevisionConflictException>(() => s.SaveAsync(p.Id, 1, J("""{"a":2}""")));
        Assert.Equal(2, e.Actual);
        Assert.Equal("""{"a":1}""", Norm((await s.GetAsync(p.Id)).Content));
    }

    [Test]
    public async Task Non_object_content_is_rejected()
    {
        var s = new FileProjectStore(TempDir());
        var p = await s.CreateAsync("P");
        await Assert.ThrowsAsync<ArgumentException>(() => s.SaveAsync(p.Id, 1, J("[1,2]")));
    }

    [Test]
    public async Task Interrupted_save_leaves_previous_revision_intact()
    {
        var root = TempDir();
        foreach (var stage in new[] { "before-write", "mid-write", "before-rename" })
        {
            var good = new FileProjectStore(root);
            var p = await good.CreateAsync("Crash " + stage);
            await good.SaveAsync(p.Id, 1, J("""{"v":"good"}"""));

            var crashing = new FileProjectStore(root) { OnStage = st => { if (st == stage) throw new IOException("simulated crash at " + st); } };
            await Assert.ThrowsAsync<IOException>(() => crashing.SaveAsync(p.Id, 2, J("""{"v":"lost"}""")));

            var after = await new FileProjectStore(root).GetAsync(p.Id);
            Assert.Equal(2, after.Revision);
            Assert.Equal("""{"v":"good"}""", Norm(after.Content));
            Assert.True(!Files(root, p.Id).Any(f => f.EndsWith(".tmp")), "temp file left behind after failed save (" + stage + ")");
        }
    }

    [Test]
    public async Task Leftover_truncated_tmp_file_is_ignored()
    {
        var root = TempDir();
        var s = new FileProjectStore(root);
        var p = await s.CreateAsync("P");
        File.WriteAllText(Path.Combine(root, "projects", p.Id, "deadbeef.tmp"), "{\"id\":\"trunc");
        var saved = await s.SaveAsync(p.Id, 1, J("""{"x":1}"""));
        Assert.Equal(2, saved.Revision);
        Assert.Equal(2, (await new FileProjectStore(root).GetAsync(p.Id)).Revision);
    }

    [Test]
    public async Task Corrupt_newest_revision_falls_back_and_is_preserved()
    {
        var root = TempDir();
        var s = new FileProjectStore(root);
        var p = await s.CreateAsync("P");
        await s.SaveAsync(p.Id, 1, J("""{"v":"two"}"""));
        var r3 = Path.Combine(root, "projects", p.Id, "r000003.json");
        File.WriteAllText(r3, "{\"id\":\"half");                       // simulate torn/corrupted newest file

        var s2 = new FileProjectStore(root);
        var head = await s2.GetAsync(p.Id);
        Assert.Equal(2, head.Revision);
        Assert.Equal("""{"v":"two"}""", Norm(head.Content));

        var next = await s2.SaveAsync(p.Id, 2, J("""{"v":"four"}"""));   // must not clobber the corrupt r3
        Assert.Equal(4, next.Revision);
        Assert.True(File.ReadAllText(r3).StartsWith("{\"id\":\"half"), "corrupt file must be preserved");
        Assert.Equal(2, (await s2.ListRevisionsAsync(p.Id)).Count(r => r.Revision <= 2));
    }

    [Test]
    public async Task Project_with_no_readable_revision_is_listed_as_damaged()
    {
        var root = TempDir();
        var s = new FileProjectStore(root);
        var p = await s.CreateAsync("P");
        File.WriteAllText(Path.Combine(root, "projects", p.Id, "r000001.json"), "garbage");
        var list = await new FileProjectStore(root).ListAsync(true);
        Assert.Equal(1, list.Count);
        Assert.True(list[0].Damaged, "damaged project must be visible, not silently dropped");
    }

    [Test]
    public async Task Rename_and_duplicate_are_independent()
    {
        var s = new FileProjectStore(TempDir());
        var a = await s.CreateAsync("Alpha");
        await s.SaveAsync(a.Id, 1, J("""{"k":1}"""));
        var renamed = await s.RenameAsync(a.Id, "Alpha 2");
        Assert.Equal("Alpha 2", renamed.Name);

        var copy = await s.DuplicateAsync(a.Id, null);
        Assert.NotEqual(a.Id, copy.Id);
        Assert.Equal("Alpha 2 (copy)", copy.Name);
        Assert.Equal(1, copy.Revision);
        Assert.Equal("""{"k":1}""", Norm(copy.Content));

        await s.SaveAsync(copy.Id, 1, J("""{"k":99}"""));
        Assert.Equal("""{"k":1}""", Norm((await s.GetAsync(a.Id)).Content));
    }

    [Test]
    public async Task Archive_hides_from_default_list_blocks_edits_and_can_be_undone()
    {
        var s = new FileProjectStore(TempDir());
        var p = await s.CreateAsync("P");
        await s.SetArchivedAsync(p.Id, true);
        Assert.Equal(0, (await s.ListAsync(false)).Count);
        Assert.Equal(1, (await s.ListAsync(true)).Count);
        await Assert.ThrowsAsync<ProjectArchivedException>(async () => await s.SaveAsync(p.Id, (await s.GetAsync(p.Id)).Revision, J("{}")));
        await Assert.ThrowsAsync<ProjectArchivedException>(() => s.RenameAsync(p.Id, "X"));
        await s.SetArchivedAsync(p.Id, false);
        Assert.Equal(1, (await s.ListAsync(false)).Count);
    }

    [Test]
    public async Task Revision_history_is_listed_and_old_revisions_readable()
    {
        var s = new FileProjectStore(TempDir());
        var p = await s.CreateAsync("P");
        await s.SaveAsync(p.Id, 1, J("""{"n":1}"""));
        await s.SaveAsync(p.Id, 2, J("""{"n":2}"""));
        var revs = await s.ListRevisionsAsync(p.Id);
        Assert.Equal(3, revs.Count);
        Assert.Equal("""{"n":1}""", Norm((await s.GetRevisionAsync(p.Id, 2)).Content));
        await Assert.ThrowsAsync<RevisionNotFoundException>(() => s.GetRevisionAsync(p.Id, 9));
    }

    [Test]
    public async Task Concurrent_saves_with_same_expected_revision_only_one_wins()
    {
        var s = new FileProjectStore(TempDir());
        var p = await s.CreateAsync("P");
        var tasks = Enumerable.Range(0, 8).Select(i => Task.Run(async () =>
        {
            try { await s.SaveAsync(p.Id, 1, J($$"""{"w":{{i}}}""")); return true; }
            catch (RevisionConflictException) { return false; }
        })).ToArray();
        var results = await Task.WhenAll(tasks);
        Assert.Equal(1, results.Count(r => r));
        Assert.Equal(2, (await s.GetAsync(p.Id)).Revision);
    }
}
