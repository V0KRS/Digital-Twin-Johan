using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Dts.Host;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Dts.Tests;

public class ProjectApiTests
{
    private static async Task<(WebApplication app, HttpClient http)> StartAsync(string dataDir)
    {
        var app = StudioHost.Build(new[] { "--urls", "http://127.0.0.1:0", "--Dts:DataDir=" + dataDir });
        await app.StartAsync();
        var addr = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
        return (app, new HttpClient { BaseAddress = new Uri(addr) });
    }

    private static StringContent Body(string json) => new(json, Encoding.UTF8, "application/json");

    [Test]
    public async Task Create_save_restart_load_over_http()   // Checkpoint-1 persistence acceptance, server side
    {
        var dir = ProjectStoreTests.TempDir();
        string id;
        var (app1, http1) = await StartAsync(dir);
        try
        {
            var created = await http1.PostAsync("/api/projects", Body("""{"name":"Reservoir"}"""));
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString()!;
            var put = await http1.PutAsync($"/api/projects/{id}", Body("""{"expectedRevision":1,"content":{"objects":[{"id":"o1","type":"box"}]}}"""));
            Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        }
        finally { await app1.StopAsync(); }

        var (app2, http2) = await StartAsync(dir);   // second process lifetime, same data dir
        try
        {
            var j = await http2.GetFromJsonAsync<JsonElement>($"/api/projects/{id}");
            Assert.Equal("Reservoir", j.GetProperty("name").GetString());
            Assert.Equal(2, j.GetProperty("revision").GetInt32());
            Assert.Equal("o1", j.GetProperty("content").GetProperty("objects")[0].GetProperty("id").GetString());
            var list = await http2.GetFromJsonAsync<JsonElement>("/api/projects");
            Assert.Equal(1, list.GetArrayLength());
        }
        finally { await app2.StopAsync(); }
    }

    [Test]
    public async Task Error_mapping_404_409_400()
    {
        var (app, http) = await StartAsync(ProjectStoreTests.TempDir());
        try
        {
            Assert.Equal(HttpStatusCode.NotFound, (await http.GetAsync("/api/projects/" + new string('a', 32))).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await http.GetAsync("/api/projects/not-an-id")).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await http.PostAsync("/api/projects", Body("""{"name":""}"""))).StatusCode);

            var c = await http.PostAsync("/api/projects", Body("""{"name":"P"}"""));
            var id = (await c.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString();
            Assert.Equal(HttpStatusCode.OK, (await http.PutAsync($"/api/projects/{id}", Body("""{"expectedRevision":1,"content":{}}"""))).StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, (await http.PutAsync($"/api/projects/{id}", Body("""{"expectedRevision":1,"content":{}}"""))).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await http.PutAsync($"/api/projects/{id}", Body("""{"expectedRevision":2,"content":[1]}"""))).StatusCode);
        }
        finally { await app.StopAsync(); }
    }

    [Test]
    public async Task Rename_duplicate_archive_endpoints()
    {
        var (app, http) = await StartAsync(ProjectStoreTests.TempDir());
        try
        {
            var c = await http.PostAsync("/api/projects", Body("""{"name":"One"}"""));
            var id = (await c.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString();
            var r = await http.PostAsync($"/api/projects/{id}/rename", Body("""{"name":"Uno"}"""));
            Assert.Equal("Uno", (await r.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("name").GetString());
            var d = await http.PostAsync($"/api/projects/{id}/duplicate", null);
            Assert.Equal(HttpStatusCode.Created, d.StatusCode);
            Assert.Equal("Uno (copy)", (await d.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("name").GetString());
            Assert.Equal(HttpStatusCode.OK, (await http.PostAsync($"/api/projects/{id}/archive", null)).StatusCode);
            Assert.Equal(1, (await http.GetFromJsonAsync<JsonElement>("/api/projects")).GetArrayLength());
            Assert.Equal(2, (await http.GetFromJsonAsync<JsonElement>("/api/projects?includeArchived=true")).GetArrayLength());
            var revs = await http.GetFromJsonAsync<JsonElement>($"/api/projects/{id}/revisions");
            Assert.Equal(3, revs.GetArrayLength());
        }
        finally { await app.StopAsync(); }
    }
}
