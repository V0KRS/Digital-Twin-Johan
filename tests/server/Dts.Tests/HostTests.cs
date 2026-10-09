using System.Net.Http.Json;
using System.Text.Json;
using Dts.Host;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Dts.Tests;

public class HostTests
{
    private static async Task<(WebApplication app, HttpClient http)> StartAsync()
    {
        var app = StudioHost.Build(new[] { "--urls", "http://127.0.0.1:0" });
        await app.StartAsync();
        var addr = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
        return (app, new HttpClient { BaseAddress = new Uri(addr) });
    }

    [Test]
    public async Task Health_returns_ok()
    {
        var (app, http) = await StartAsync();
        try
        {
            var r = await http.GetAsync("/api/health");
            Assert.Equal(System.Net.HttpStatusCode.OK, r.StatusCode);
            var j = await r.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("ok", j.GetProperty("status").GetString());
        }
        finally { await app.StopAsync(); }
    }

    [Test]
    public async Task Version_reports_product_and_schema()
    {
        var (app, http) = await StartAsync();
        try
        {
            var j = await http.GetFromJsonAsync<JsonElement>("/api/version");
            Assert.Equal("Industrial Digital Twin Studio", j.GetProperty("product").GetString());
            Assert.Equal(1, j.GetProperty("configSchemaVersion").GetInt32());
        }
        finally { await app.StopAsync(); }
    }

    [Test]
    public async Task Unknown_route_is_404()
    {
        var (app, http) = await StartAsync();
        try { Assert.Equal(System.Net.HttpStatusCode.NotFound, (await http.GetAsync("/nope")).StatusCode); }
        finally { await app.StopAsync(); }
    }
}
