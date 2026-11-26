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
