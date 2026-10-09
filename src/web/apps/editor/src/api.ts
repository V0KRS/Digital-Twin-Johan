export interface VersionInfo {
  product: string;
  version: string;
  configSchemaVersion: number;
}

export class ApiError extends Error {
  constructor(message: string, readonly status?: number) {
    super(message);
    this.name = 'ApiError';
  }
}

async function getJson<T>(path: string, fetchImpl: typeof fetch): Promise<T> {
  let res: Response;
  try {
    res = await fetchImpl(path, { headers: { Accept: 'application/json' } });
  } catch {
    throw new ApiError('Server unreachable');
  }
  if (!res.ok) throw new ApiError(`Request to ${path} failed`, res.status);
  return (await res.json()) as T;
}

export const getHealth = (f: typeof fetch = fetch) => getJson<{ status: string }>('/api/health', f);
export const getVersion = (f: typeof fetch = fetch) => getJson<VersionInfo>('/api/version', f);
