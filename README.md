# Industrial Digital Twin Studio

Internal platform for building reusable industrial digital twins (water/PUB projects and beyond).
Status: **Checkpoint 1, increment 1b (project persistence + API) done**. See `CONTINUATION.md` for exact state.

## Prerequisites
- .NET SDK 10 (LTS) — pinned via `global.json`
- Node.js 22+ and npm
- (Later checkpoints) PostgreSQL; Docker is optional, never required.

## Run (development)
```bash
dotnet run --project src/server/Dts.Host          # http://127.0.0.1:5080 (loopback only by default)
cd src/web && npm ci && npm run dev -w apps/editor  # Vite dev server, proxies /api to the host
```
Endpoints now: `GET /api/health`, `GET /api/version`, and `/api/projects` (list, create, get, save with
`expectedRevision`, rename, duplicate, archive/unarchive, revisions). Projects are stored as files under
`Dts:DataDir` (default: per-user local app data); override with `--Dts:DataDir=/path`.

## Verify everything
```bash
./scripts/verify.sh
```

## Layout
`src/server` (ASP.NET Core modular monolith) · `src/web` (React + TypeScript; Babylon.js arrives in 1c) ·
`tests/` · `docs/` · `deploy/` · `scripts/`

## Notes
- Offline-capable by design: no CDN or internet dependency at runtime.
- Server binds to loopback by default; network exposure is an explicit configuration choice.
- Read-only toward industrial systems (no write path exists in the connector design).
