import { useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../../api-client';
import { api } from '../../../../platform/api';
import { moduleKeys } from '../../../../platform/query-keys';

export type GiftsSummaryView = Schema<'GiftsSummary'>;
export type GiftCounts = Schema<'GiftCounts'>;
export type Recipient = Schema<'Recipient'>;

/**
 * The Gifts section at a glance (`GET /api/gifts/summary`). The key sits under the Gifts prefix (`moduleKeys.gifts`),
 * which every Gifts write refreshes (web ADR 0009), so the summary is fresh after a change in the section.
 */
export function useGiftsSummary() {
  return useQuery({
    queryKey: [...moduleKeys.gifts, 'summary'],
    queryFn: ({ signal }) => apiCall(api.GET('/api/gifts/summary', { signal })),
  });
}
