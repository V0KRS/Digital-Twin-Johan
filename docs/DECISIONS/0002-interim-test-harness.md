# 0002 Interim server test harness
Status: interim. NuGet (api.nuget.org) was blocked (HTTP 403) in the authoring sandbox, so xUnit/Testcontainers could
not be restored. Server tests use a small dependency-free runner (`tests/server/Dts.Tests`), exercising the host
in-process over real HTTP. Server code uses only the ASP.NET Core shared framework for the same reason.
Action: when NuGet is reachable (developer machine/CI), migrate tests to xUnit and add Npgsql/EF Core in 1b.
