import { useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../../api-client';
import { api } from '../../../../platform/api';

export type ShoppingSummary = Schema<'ShoppingSummary'>;
export type SummaryItem = ShoppingSummary['nextItems'][number];

/** The shopping list at a glance (GET /api/shopping/summary); shares the `shopping` key prefix with the section. */
export function useShoppingSummary() {
  return useQuery({
    queryKey: ['shopping', 'summary'],
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/shopping/summary', { signal })),
  });
}
