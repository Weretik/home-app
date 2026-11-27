import { ApiError, NetworkError } from '../../api-client';
import { MutationCache, QueryClient } from '@tanstack/react-query';
import { markOffline } from './connectivity';

/** How long unused data stays in memory: as long as it is kept for offline use (web ADR 0022). */
export const QUERY_GC_TIME = 7 * 24 * 60 * 60 * 1000;

/**
 * A read that got no answer counts the app offline and waits for the network, keeping the data it has (no error is
 * shown); otherwise only failures that may pass on their own are retried, as a 4xx answer will not change on retry.
 */
export function retryRead(failureCount: number, error: Error): boolean {
  if (error instanceof NetworkError) {
    markOffline();
    return true;
  }
  return !(error instanceof ApiError && error.status < 500) && failureCount < 2;
}

/**
 * Reads retry by {@link retryRead}; a write that got no answer also counts the app offline. Data stays a week, so the
 * offline cache can show it. Writes fail at once offline (`networkMode: 'always'`) unless they are queued writes
 * (src/platform/offline), which pause until the network is back.
 */
export function createQueryClient(): QueryClient {
  return new QueryClient({
    mutationCache: new MutationCache({
      onError: (error) => {
        if (error instanceof NetworkError) markOffline();
      },
    }),
    defaultOptions: {
      mutations: { networkMode: 'always' },
      queries: {
        staleTime: 30_000,
        gcTime: QUERY_GC_TIME,
        retry: retryRead,
      },
    },
  });
}

/**
 * The query key prefix of each backend module. Every read of a module's API starts its key with the prefix, and every
 * write to the module invalidates the whole prefix, so each page and summary that shows the module's data refreshes,
 * whichever feature shows it (features never import each other; docs/architecture.md, "App").
 */
export const moduleKeys = {
  expenses: ['expenses'],
  shopping: ['shopping'],
  gifts: ['gifts'],
  todos: ['todos'],
  notifications: ['notifications'],
  identity: ['identity'],
  notes: ['notes'],
} as const;
