#!/usr/bin/env bash
# Full local verification: server build+tests, web typecheck+tests+build. Exits non-zero on any failure.
set -euo pipefail
cd "$(dirname "$0")/.."
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
echo "== server build";  dotnet build DigitalTwinStudio.slnx -c Release
echo "== server tests";  dotnet run --project tests/server/Dts.Tests -c Release --no-build
echo "== web install";   (cd src/web && npm ci --no-audit --no-fund)
echo "== web typecheck"; (cd src/web && npm run typecheck)
echo "== web tests";     (cd src/web && npm test)
echo "== web build";     (cd src/web && npm run build)
echo "ALL CHECKS PASSED"
