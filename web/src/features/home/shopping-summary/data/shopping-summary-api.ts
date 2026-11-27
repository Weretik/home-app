import { useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../../api-client';
import { api } from '../../../../platform/client/api';
import { moduleKeys } from '../../../../platform/client/query-client';

export type ShoppingSummary = Schema<'ShoppingSummary'>;
export type SummaryItem = ShoppingSummary['nextItems'][number];

/**
 * The shopping list at a glance (GET /api/shopping/summary), under the Shopping prefix (`moduleKeys.shopping`) that
 * every Shopping write refreshes (web ADR 0009).
 */
export function useShoppingSummary() {
  return useQuery({
    queryKey: [...moduleKeys.shopping, 'summary'],
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/shopping/summary', { signal })),
  });
}
