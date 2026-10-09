# CLAUDE.md — Industrial Digital Twin Studio

Internal platform for building reusable industrial digital twins (water/PUB projects in Singapore and
other industrial sites): 3D scene editor + runtime, SCADA telemetry (read-only), reusable equipment
library, alarms, trends, portable project packages.

**Status:** Checkpoint 0 (architecture) is APPROVED and complete. Do NOT write another architecture
proposal, restart the project, or generate a new skeleton. The architecture is in `docs/ARCHITECTURE.md`
and `docs/DECISIONS/`; change it only for a concrete technical reason, recorded as a new decision file.

Reference material (read only the sections relevant to the current task, not the whole file):
- `docs/MASTER_SPEC.md` — full product requirements and the checkpoint roadmap (Checkpoints 1-9)
- `docs/REQUIREMENTS.md` — acceptance criteria and status
- `CONTINUATION.md` — the live project state. Always read it first.

---

## 1. What "continue" means

The user sends either just "continue", or a repo link plus "continue" / "read CLAUDE.md and
CONTINUATION.md". Both mean the same thing. The user will not repeat requirements or name the next task.

1. **Get the repo.** Use the current working copy if inside one; otherwise `git clone <link>`.
   If the clone fails (private repo, no network), ask the user to upload a ZIP. Never continue from
   memory or recreate files from conversation history.
2. **Read** `CLAUDE.md`, then `CONTINUATION.md`, then `docs/ARCHITECTURE.md`. Read other docs only as needed.
3. **Establish the real state:** `git status`, `git log --oneline -10`, then build and test
   (`./scripts/verify.sh`, or the fast subset if it is too slow). Do not trust earlier claims.
4. **Check the environment** and record it in CONTINUATION.md: dotnet/node versions, whether NuGet and npm
   are reachable, whether PostgreSQL/Docker exist, whether git can push. Work around limits, and say which
   verification could not be run.
5. **Choose the task** using the priority order below. Default: "Next recommended task" in CONTINUATION.md.
6. **Implement, verify, document, deliver** (sections 3-5).

## 2. Priority order and sizing

1. Broken builds, startup failures, regressions.
2. Defects blocking the current feature.
3. Finish the current increment (including its tests).
4. Missing acceptance tests and verification.
5. The next vertical slice toward the current checkpoint.
6. Documentation and maintainability.
7. Enhancements.

- One increment = one coherent vertical slice that is implemented, tested and documented (e.g. project store +
  CRUD API; scene hierarchy + selection). Not tiny, not a whole checkpoint.
- Prefer a smaller verified change over a larger unverified one.
- Always leave enough budget to update CONTINUATION.md, commit and deliver. If budget is running low, stop at
  the last green commit and write the notes.
- Do not start many unfinished changes at once.

## 3. Git workflow

- Work on a branch per increment: `inc/<id>-<name>` (e.g. `inc/1b-project-store`).
- Make small commits, one per green step (interface, implementation, tests).
- Before a large edit, add an "In progress" section to CONTINUATION.md (what is changing, files involved, what is
  missing) and commit it. Remove it when done.
- Run the fast checks (build + tests of the touched area) before every code commit. Run the full
  `./scripts/verify.sh` before merging to `main` or delivering. Never commit code that does not build.
- Never commit secrets, tokens, certificates or real site data.

### Delivery depends on the environment. Detect it; do not assume.

**A. Git can push** (e.g. Claude Code with the user's credentials): push the branch after each commit; merge to
`main` and push once the full verify passes.

**B. Git cannot push** (e.g. browser chat sandbox; test with `git push --dry-run`, never prompt the user to paste
tokens): commit locally, then produce deliverables in the outputs folder and present them:
- `git format-patch origin/main..HEAD -o <outputs>/patches` (one patch per commit)
- `git bundle create <outputs>/dts-<increment>.bundle origin/main..HEAD`
- Give the user the exact apply commands:
  `git checkout main && git pull && git am <patches>\*.patch && git push`
- Say plainly that nothing has been pushed.

Never claim anything was pushed or saved unless you verified it.

## 4. What to update at the end of every increment

- [ ] `CONTINUATION.md` (see template below). Always.
- [ ] Tests for the new behaviour, in the same change as the code.
- [ ] `README.md` if prerequisites, commands or run steps changed.
- [ ] `docs/ARCHITECTURE.md` if structure or behaviour changed (mark planned vs implemented).
- [ ] `docs/REQUIREMENTS.md`: update acceptance status for the checkpoint.
- [ ] `docs/DECISIONS/NNNN-title.md` for any significant decision or deviation (including environment-driven ones).
- [ ] `scripts/verify.sh` if new projects or test suites were added.
Update existing docs instead of creating duplicates.

### CONTINUATION.md required headings
Persistence status · Objective · Approved architecture/decisions (pointer) · Current checkpoint and increment ·
Implemented · Build/test commands · Environment notes · Verification actually performed (with date) ·
Known issues · Unimplemented requirements · Blockers · Important files changed · In progress (only if unfinished) ·
**Next recommended task** (specific, with acceptance criteria).
Keep it concise: another AI session must be able to resume from it alone.

## 5. Definition of done

A feature is done only when it is implemented, integrated, persisted where relevant, and tested. A button must do
what it says; a saved project must survive restart; an imported model must appear and be editable; a mapping must
change the intended property; a disconnected source must not look healthy; simulated data must be labelled.
A class, endpoint, table or screen existing is not "done". No placeholder implementations returning hardcoded success.

## 6. Non-negotiable rules

- **Read-only toward real PLCs/SCADA.** No write path in v1. The twin is not a safety system or a replacement for
  the plant's control or alarm systems.
- **Honest data states.** Distinguish live, simulated, stale, bad-quality, unavailable. Never show old values as current.
- **No real PUB data or credentials** in the repo, samples, logs, exports or diagnostics. Sample data is illustrative.
- **Never disable TLS/certificate validation.** Secure defaults (loopback bind etc.). Offline-capable, no CDN.
- **Behaviours are declarative data;** imported projects must not execute arbitrary code.
- **Alarm processing is independent of rendering;** definitions vs instances stay separate; library updates never
  silently overwrite project values.
- **Do not claim support** for a format, protocol or capacity until implemented and tested/measured.
- **Report test results truthfully:** passed / failed / not run / could not run (and why) / manual checks actually done.

## 7. Stack summary (details in docs/ARCHITECTURE.md)

Babylon.js + React/TypeScript (web); ASP.NET Core modular monolith on .NET 10 LTS; PostgreSQL; SignalR; OPC UA then
MQTT connectors; `.dtpkg` project packages. Deployment: single workstation first, Docker and Windows-native both
eventually (Docker never mandatory); later a shared intranet server. GLB/glTF native; FBX/IFC via documented
conversion. Chromium + WebGL2 assumed but not hardcoded. Pin dependency versions; verify licences at adoption.

## 8. End every response with this report

1. What was implemented. 2. Important files changed. 3. Commands actually run. 4. Actual results.
5. Known defects and unverified items. 6. Progress against the current checkpoint.
7. Delivery status (pushed / patch+bundle provided, and the apply commands). 8. The next task.

Ask the user a question only if a decision genuinely blocks progress and has no reasonable documented default.
