import { useInfiniteQuery, infiniteQueryOptions } from '@tanstack/react-query';
import { apiCall } from '../../../api-client';
import { api } from '../../../platform/client/api';
import { shoppingKey } from '../data/shopping-api';

/** History rows per request; the API allows up to 100. */
export const HISTORY_PAGE = 50;

interface Paged {
  page: number;
  pageSize: number;
  totalCount: number;
}

/** The next page number, or undefined after the last page. */
export const nextPage = (last: Paged) =>
  last.page * last.pageSize < last.totalCount ? last.page + 1 : undefined;

/** Bought items that left the cart, newest first, page by page («Показати ще»). */
/** The history's query, also used to load it ahead for offline use (web ADR 0022). */
export const shoppingHistoryQuery = () =>
  infiniteQueryOptions({
    queryKey: [...shoppingKey, 'history'],
    initialPageParam: 1,
    getNextPageParam: nextPage,
    queryFn: ({ pageParam, signal }) =>
      apiCall(
        api.GET('/api/shopping/history', {
          params: { query: { page: pageParam, pageSize: HISTORY_PAGE } },
          signal,
        }),
      ),
  });

export function useShoppingHistory() {
  return useInfiniteQuery(shoppingHistoryQuery());
}
