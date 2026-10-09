import { describe, expect, it } from 'vitest';
import { ApiError, getVersion } from './api';

const json = (body: unknown, status = 200) =>
  (async () => new Response(JSON.stringify(body), { status })) as unknown as typeof fetch;

describe('api client', () => {
  it('parses version info', async () => {
    const v = await getVersion(json({ product: 'P', version: '1', configSchemaVersion: 1 }));
    expect(v.configSchemaVersion).toBe(1);
  });
  it('throws ApiError with status on HTTP failure', async () => {
    await expect(getVersion(json({}, 500))).rejects.toMatchObject({ name: 'ApiError', status: 500 });
  });
  it('throws ApiError when server unreachable', async () => {
    const f = (async () => { throw new TypeError('network'); }) as unknown as typeof fetch;
    await expect(getVersion(f)).rejects.toBeInstanceOf(ApiError);
  });
});
