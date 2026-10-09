import { cleanup, render, screen, waitFor } from '@testing-library/react';
import { afterEach, describe, expect, it } from 'vitest';
import { App } from './App';

afterEach(cleanup);

describe('App', () => {
  it('shows online status with server version', async () => {
    const f = (async () =>
      new Response(JSON.stringify({ product: 'P', version: '0.1.0', configSchemaVersion: 1 }))) as unknown as typeof fetch;
    render(<App fetchImpl={f} />);
    await waitFor(() => expect(screen.getByTestId('server-status').textContent).toContain('Server online'));
  });
  it('shows offline status, not a healthy one, when the server is down', async () => {
    const f = (async () => { throw new TypeError('x'); }) as unknown as typeof fetch;
    render(<App fetchImpl={f} />);
    await waitFor(() => expect(screen.getByTestId('server-status').textContent).toContain('Server offline'));
  });
});
