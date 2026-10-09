# CONTINUATION — Industrial Digital Twin Studio

## Objective
Reusable industrial digital twin platform for PUB/water projects (editor + runtime, SCADA read-only, reusable library,
alarms/trends, portable project packages). Source of truth: https://github.com/V0KRS/Digital-Twin-Johan

## Persistence status (be honest)
- Source of truth is the GitHub repo. The sandbox that authored 1b had **no push credentials** (`git push --dry-run`:
  "could not read Username"). 1b is committed on local branch `inc/1b-project-store` and delivered as patches + bundle.
  **Nothing from 1b has been pushed** until you apply it (commands in the delivery message / below).
- Apply: `git checkout main && git pull && git checkout -b inc/1b-project-store && git am patches/*.patch`
  (or `git pull dts-1b.bundle inc/1b-project-store`), run `./scripts/verify.sh`, then merge to main and push.

## Approved architecture and decisions
See docs/ARCHITECTURE.md and docs/DECISIONS/. Babylon.js + React/TS; ASP.NET Core modular monolith on .NET 10 LTS;
PostgreSQL; SignalR; OPC UA then MQTT, read-only; declarative behaviours; .dtpkg packages.
User answers: Docker and Windows-native both eventually (Docker never mandatory); single workstation first;
OPC UA not assumed at every site; GLB native, FBX/IFC via documented conversion; Chromium+WebGL2 assumed, offline-capable.

## Current checkpoint
Checkpoint 1: 1a (skeleton) complete; **1b (project store + CRUD API) complete server-side**; 1c, 1d not started.

## Implemented
- 1a: `Dts.Domain`, `Dts.Host` (health/version, JSON logging, loopback 127.0.0.1:5080), `Dts.Tests` (interim runner),
  web workspace `src/web` (React editor shell, typed API client, Vite proxy), `scripts/verify.sh`.
- 1b: `Dts.Domain/Projects` (ProjectDocument, summaries, exceptions, `IProjectStore`, name validation);
  `Dts.Host/Persistence/FileProjectStore` (decision 0003); `Dts.Host/ProjectEndpoints.cs` with
  `GET/POST /api/projects`, `GET/PUT /api/projects/{id}` (PUT needs `expectedRevision`, 409 on stale),
  `POST .../rename|duplicate|archive|unarchive`, `GET .../revisions[/{n}]`; exception->404/409/400 middleware.
  Config `Dts:DataDir`. Project `content` is an opaque JSON object (scene schema is for 1c/2).

## Build and test commands
`./scripts/verify.sh` (server build, server tests, npm ci, web typecheck, web tests, web build).
Run: `dotnet run --project src/server/Dts.Host` and `cd src/web && npm run dev -w apps/editor`.

## Environment notes (2026-10-09 sandbox)
dotnet SDK was absent; installed `dotnet-sdk-10.0` 10.0.112 via apt (works, matches global.json rollForward). Node 22.22.2,
npm 10.9.7. NuGet (api.nuget.org) still blocked (403). No psql/docker. git push not possible (no credentials).

## Verification actually performed (2026-10-09)
- `./scripts/verify.sh` on the 1a baseline before changes: exit 0.
- After 1b: server build 0 warnings/0 errors; server tests **20 passed, 0 failed** (3 host, 3 project API over real HTTP
  incl. stop/start restart, 14 store tests incl. fault-injected interrupted save at 3 stages, leftover tmp, corrupt newest
  revision fallback, damaged-project visibility, stale-revision conflict, 8-way concurrent save, path traversal, archive).
- Manual: real host process, create + save, `kill -9`, restart, GET returned identical content at revision 2; stale PUT -> 409.
- Full `./scripts/verify.sh` re-run on the final 1b commit: exit 0, "ALL CHECKS PASSED" (server 20/20, web 5/5, build OK).
- NOT verified: Windows, Docker, browser/editor, real power-loss durability (directory fsync not performed),
  multi-process access, large projects.

## Known issues / limitations
- NuGet blocked in sandbox -> no xUnit/EF Core/Npgsql/Serilog yet; interim harness (DECISIONS/0002). Migrate to xUnit
  on a machine with NuGet.
- PostgreSQL store not implemented; file store has no revision pruning, no dir fsync, single-process only (0003).
- No auth on /api/projects (loopback bind only). Required before any network exposure (Checkpoint 8).
- No request size limit tuning on PUT (Kestrel default ~30 MB).
- Library licences (Babylon.js, OPC UA stack, MQTTnet) unverified.
- TypeScript resolved to 7.0.2 and Vitest 5.0.3: new majors; watch for tooling surprises.
- No Dockerfile, installer or auth yet.

## Unimplemented requirements
Everything beyond 1b: editor project UI, 3D, import, library, tags, connectors, alarms, history,
packaging, security, deployment.

## Blockers
- GitHub push needs your credentials (see Persistence status).

## Important files changed in 1b
src/server/Dts.Domain/Projects/*, src/server/Dts.Host/Persistence/FileProjectStore.cs, src/server/Dts.Host/ProjectEndpoints.cs,
src/server/Dts.Host/StudioHost.cs, tests/server/Dts.Tests/{ProjectStoreTests,ProjectApiTests,MiniTest}.cs,
docs/DECISIONS/0003-file-project-store.md, docs/ARCHITECTURE.md, docs/REQUIREMENTS.md, README.md.

## Next recommended task: 1c
Editor project UI + Babylon.js viewport with one basic object. Add to `apps/editor`: typed client for /api/projects;
project list (create/open/rename/duplicate/archive); open project shows a Babylon.js (pin version, check licence,
offline bundle, no CDN) WebGL2 viewport; "Add box" adds an object `{id,type:"box",position,...}` to the project's
`content.objects`; Save button calls PUT with expectedRevision and handles 409 (show conflict, offer reload); unsaved-
changes indicator. Acceptance: vitest tests for client + project state logic (viewport itself behind a thin interface so
it can be faked in jsdom); `verify.sh` green; document what could not be checked without a browser (WebGL rendering).
Then 1d: restart acceptance (create, add object, save, restart server, reload in UI) with an automated end-to-end test
if a browser (Playwright/Chromium) is obtainable, otherwise a documented manual script.
