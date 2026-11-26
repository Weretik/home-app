import { useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../../api-client';
import { api } from '../../../../platform/api';

export type GiftsSummaryView = Schema<'GiftsSummary'>;
export type GiftCounts = Schema<'GiftCounts'>;
export type Recipient = Schema<'Recipient'>;

/**
 * The Gifts section at a glance (`GET /api/gifts/summary`). The key sits under `['gifts']`, and every Gifts write
 * refreshes `['gifts', 'summary']`, so the summary is fresh after a change in the section.
 */
export function useGiftsSummary() {
  return useQuery({
    queryKey: ['gifts', 'summary'],
    queryFn: ({ signal }) => apiCall(api.GET('/api/gifts/summary', { signal })),
  });
}
