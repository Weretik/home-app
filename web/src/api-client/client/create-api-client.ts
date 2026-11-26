import createClient from 'openapi-fetch';
import type { paths } from '../generated/api';

export interface ApiClientOptions {
  /** The API origin, for example `https://api.example.com`; empty for the same origin. */
  baseUrl: string;
  /** Replaces the global fetch, for example to add Next.js caching options. */
  fetch?: typeof globalThis.fetch;
}

/**
 * The typed client of the backend API. Paths, parameters and bodies come from the OpenAPI document the backend
 * generates, so a breaking backend change fails the frontend type check instead of production.
 */
export function createApiClient({ baseUrl, fetch }: ApiClientOptions) {
  return createClient<paths>({
    baseUrl,
    credentials: 'include',
    // Looked up on every call, not once at start, so tests can stub the global fetch.
    fetch: fetch ?? ((request) => globalThis.fetch(request)),
  });
}

export type ApiClient = ReturnType<typeof createApiClient>;
