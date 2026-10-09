# 0003 File-based project store behind IProjectStore (increment 1b)
Status: accepted (interim engine; PostgreSQL implementation still planned per 0001).
Context: PostgreSQL and NuGet were unavailable in the authoring sandbox, and a single-workstation install should not
need a database to save a project.
Decision: `IProjectStore` (Dts.Domain) with `FileProjectStore` (Dts.Host/Persistence). Layout
`{DataDir}/projects/{id}/r{revision:D6}.json`, one immutable full snapshot per revision (save, rename, archive each
create one). Save = write `*.tmp`, flush to disk, atomic rename without overwrite. Current state = highest revision that
parses and validates; a corrupt newest file is kept, skipped, logged, and new revisions are numbered above it.
Optimistic concurrency via `expectedRevision` (409 on mismatch). Ids are 32 hex chars (blocks path traversal).
Project content is opaque JSON object; scene schema arrives in 1c/2.
Consequences: no pruning of old revisions yet; directory is not fsynced (last rename may be lost on power failure,
previous revision then remains current); single-process only (in-process lock, no cross-process file lock);
whole-file read per Get. A PostgreSQL store can implement the same interface later; no data migration is
promised yet. Data dir: `Dts:DataDir` config, default `%LOCALAPPDATA%/DigitalTwinStudio` (Linux: `~/.local/share/...`).
