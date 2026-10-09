# Architecture (approved at Checkpoint 0)

Hybrid, browser-first: **Babylon.js + React/TypeScript** clients, **ASP.NET Core** modular monolith,
**PostgreSQL** (config as versioned JSONB, history in partitioned tables), **SignalR** for realtime,
OPC UA (OPC Foundation stack) then MQTT (MQTTnet) connectors — **read-only** by construction.

Key rules
- Alarm evaluation is server-side on its own durable queue; rendering is decoupled from ingestion.
- Equipment *definitions* are versioned; *instances* store definitionVersion + override delta.
- Projects export as `.dtpkg` (zip + manifest, no secrets, no absolute paths). Behaviours are declarative JSON.
- Quality model: Good | Uncertain | Bad | Stale | Unavailable | Unknown | Simulated.
- Deployment: single workstation first (Windows-native and Docker both eventually), intranet server later.

Implemented so far: (1a) `Dts.Domain` (product identity), `Dts.Host` (health/version, JSON logging, loopback bind),
web editor shell. (1b) project model + `IProjectStore` + crash-safe `FileProjectStore` (decision 0003) and
`/api/projects` CRUD/revision endpoints. PostgreSQL store, scene content, 3D, auth: planned, not built.
Full proposal: Checkpoint 0 response (conversation); decisions in `docs/DECISIONS/`.
