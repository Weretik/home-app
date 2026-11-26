/**
 * The API origin comes from VITE_API_URL at build time (the front and the API live on different hosts).
 * Every API call goes through `getJson`, so the origin and error handling live in one place.
 */
const apiUrl = (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, '');

export class ApiError extends Error {
  constructor(
    readonly status: number,
    message: string,
  ) {
    super(message);
  }
}

/** Reads JSON. A 503 from /health still has a useful body, so callers may accept extra statuses. */
export async function getJson<T>(
  path: string,
  options: { acceptStatuses?: number[]; signal?: AbortSignal } = {},
): Promise<T> {
  const response = await fetch(`${apiUrl}${path}`, { signal: options.signal });
  if (!response.ok && !options.acceptStatuses?.includes(response.status)) {
    throw new ApiError(response.status, `API ответил ${response.status}`);
  }
  return (await response.json()) as T;
}
