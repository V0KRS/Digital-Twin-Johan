# Requirements and acceptance (summary)

MVP slice: import GLB -> place library pumps/tanks -> bulk-map simulated tags -> states and alarms respond ->
export package -> create second project from it.

| Checkpoint | Acceptance |
|---|---|
| 1 Skeleton | Create project, add basic object, save, restart, load |
| 2 Import/scene | Import model, edit placement, save, restart, verify |
| 3 Library | Pump definition, N instances, customise one, others independent |
| 4 Simulated telemetry | Simulate pump/tank, map, 3D state changes |
| 5 OPC UA | Connect to test server, subscribe, detect loss, recover |
| 6 Runtime/alarms/trends | Controlled telemetry -> verified values, events, timestamps |
| 7 Reuse | Second project from template keeps behaviours, independent config |
| 8 Security/deploy | Clean-machine install, simulator, restore backup |
| 9 RC | Release-readiness report |

Checkpoint 1 increments: 1a skeleton (done) · 1b persistence + project CRUD · 1c editor shell + Babylon viewport · 1d restart acceptance.

## Status
| Item | Status |
|---|---|
| 1a skeleton | done, verified |
| 1b project create/rename/duplicate/archive, atomic save with revisions, schemaVersion | done server-side (file store); verified by tests 2026-10-09 |
| 1b crash-safety (interrupted save keeps previous revision) | done; fault-injection tests plus manual `kill -9` restart |
| 1b PostgreSQL store | not started (needs NuGet + Postgres) |
| Checkpoint 1 acceptance (add object, save, restart, load in the UI) | NOT met: no editor UI/viewport yet (1c, 1d) |
