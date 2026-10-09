import { useEffect, useState } from 'react';
import { getVersion, type VersionInfo } from './api';

type ServerState =
  | { kind: 'checking' }
  | { kind: 'online'; info: VersionInfo }
  | { kind: 'offline'; reason: string };

export function App({ fetchImpl }: { fetchImpl?: typeof fetch }) {
  const [state, setState] = useState<ServerState>({ kind: 'checking' });

  useEffect(() => {
    let cancelled = false;
    getVersion(fetchImpl)
      .then((info) => !cancelled && setState({ kind: 'online', info }))
      .catch((e: Error) => !cancelled && setState({ kind: 'offline', reason: e.message }));
    return () => {
      cancelled = true;
    };
  }, [fetchImpl]);

  return (
    <main style={{ fontFamily: 'system-ui, sans-serif', padding: 24 }}>
      <h1>Industrial Digital Twin Studio</h1>
      <p role="status" data-testid="server-status">
        {state.kind === 'checking' && 'Checking server…'}
        {state.kind === 'online' && `Server online — v${state.info.version} (config schema ${state.info.configSchemaVersion})`}
        {state.kind === 'offline' && `Server offline: ${state.reason}`}
      </p>
    </main>
  );
}
