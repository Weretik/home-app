import { useQuery, queryOptions } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../../api-client';
import { api } from '../../../../platform/client/api';
import { moduleKeys } from '../../../../platform/client/query-client';

export type GiftsSummaryView = Schema<'GiftsSummary'>;
export type GiftCounts = Schema<'GiftCounts'>;
export type Recipient = Schema<'Recipient'>;

/** The query of `useGiftsSummary`, also used to load it ahead for offline use (web ADR 0022). */
export const giftsSummaryQuery = () =>
  queryOptions({
    queryKey: [...moduleKeys.gifts, 'summary'],
    queryFn: ({ signal }) => apiCall(api.GET('/api/gifts/summary', { signal })),
  });

/**
 * The Gifts section at a glance (`GET /api/gifts/summary`). The key sits under the Gifts prefix (`moduleKeys.gifts`),
 * which every Gifts write refreshes (web ADR 0009), so the summary is fresh after a change in the section.
 */
export function useGiftsSummary() {
  return useQuery({ ...giftsSummaryQuery() });
}
