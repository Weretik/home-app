import { ApiError } from '../api-client';
import { QueryClient } from '@tanstack/react-query';

/** Retries only failures that may pass on their own; a 4xx answer will not change on retry. */
export function createQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 30_000,
        retry: (failureCount, error) =>
          !(error instanceof ApiError && error.status < 500) &&
          failureCount < 2,
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
} as const;
