# CONTINUATION — Industrial Digital Twin Studio

## Objective
Reusable industrial digital twin platform for PUB/water projects (editor + runtime, SCADA read-only, reusable library,
alarms/trends, portable project packages). Source of truth: https://github.com/V0KRS/Digital-Twin-Johan

## Persistence status (be honest)
- Authoring sandbox had **no GitHub credentials**: `git ls-remote` worked (repo readable, empty) but push fails
  ("could not read Username"). The sandbox filesystem **does not persist** between sessions.
- Work exists as a local git repo delivered as ZIP + git bundle. **Nothing has been pushed to GitHub.**
- To resume: push the bundle to GitHub yourself, then give the next session the repo (upload ZIP or attach repo).

## Approved architecture and decisions
See docs/ARCHITECTURE.md and docs/DECISIONS/. Babylon.js + React/TS; ASP.NET Core modular monolith on .NET 10 LTS;
PostgreSQL; SignalR; OPC UA then MQTT, read-only; declarative behaviours; .dtpkg packages.
User answers: Docker and Windows-native both eventually (Docker never mandatory); single workstation first;
OPC UA not assumed at every site; GLB native, FBX/IFC via documented conversion; Chromium+WebGL2 assumed, offline-capable.

## Current checkpoint
Checkpoint 1, increment **1a (skeleton) — complete**. 1b/1c/1d not started.

## Implemented
- Solution `DigitalTwinStudio.slnx`: `Dts.Domain` (ProductInfo), `Dts.Host` (GET /api/health, GET /api/version,
  JSON console logging, loopback bind 127.0.0.1:5080), `Dts.Tests` (interim runner).
- Web workspace `src/web` (npm workspaces): `apps/editor` React shell showing server online/offline status,
  typed API client, Vite proxy for /api.
- `scripts/verify.sh` runs everything.

## Build and test commands
`./scripts/verify.sh` (server build, server tests, npm ci, web typecheck, web tests, web build).
Run: `dotnet run --project src/server/Dts.Host` and `cd src/web && npm run dev -w apps/editor`.

## Verification actually performed (sandbox, 2026-10-09)
- `./scripts/verify.sh`: exit 0. Server build 0 warnings/0 errors (TreatWarningsAsErrors on).
- Server tests: 3 passed, 0 failed (health, version, 404; in-process Kestrel over real HTTP).
- Web: typecheck OK; vitest 5/5 passed; production build OK.
- Manual: ran compiled host, curl /api/health and /api/version returned 200; log confirmed bind to 127.0.0.1:5080.
- NOT verified: Windows run, Docker, any browser rendering of the editor, `npm run dev` proxy against live host.

## Known issues / limitations
- NuGet blocked in sandbox -> no xUnit/EF Core/Npgsql/Serilog yet; interim harness (DECISIONS/0002). On a machine
  with NuGet, migrate tests to xUnit before 1b.
- PostgreSQL not available in sandbox (no psql/docker); 1b persistence tests must use a real Postgres elsewhere
  or the next sandbox must find another way.
- Library licences (Babylon.js, OPC UA stack, MQTTnet) unverified.
- TypeScript resolved to 7.0.2 and Vitest 5.0.3: new majors; watch for tooling surprises.
- No Dockerfile, installer, auth or persistence yet. No project concept yet.

## Unimplemented requirements
Everything beyond skeleton: persistence, projects, 3D, import, library, tags, connectors, alarms, history,
packaging, security, deployment.

## Blockers
- GitHub push needs your credentials (see Persistence status).

## Next recommended task: 1b
Project domain model + persistence + CRUD API (create/open/rename/duplicate/archive), atomic save with revisions,
schemaVersion. Decide persistence engine given sandbox limits: implement behind an `IProjectStore` interface; first
a file-based crash-safe store (write temp + fsync + atomic rename, revision history) that runs and is testable
anywhere, with the PostgreSQL implementation added once NuGet+Postgres are reachable. Acceptance: create/rename/
duplicate project, save, restart process, reload identical; interrupted-save test leaves previous revision intact.
