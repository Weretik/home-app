import {
  useInfiniteQuery,
  useQuery,
  queryOptions,
  infiniteQueryOptions,
} from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/client/api';
import { moduleKeys } from '../../../platform/client/query-client';
import { nextPage, PAGE_SIZE } from './expenses-api';
import type { Month } from './month';

export type SavingsMovementItem = Schema<'SavingsMovementItem'>;
export type SavingsMonthSummary = Schema<'SavingsMonthSummary'>;
export type SavingsDirection = Schema<'SavingsDirection'>;

/** Every savings query starts with this key, under the module's prefix. */
const savingsKey = [...moduleKeys.expenses, 'savings'] as const;

/** One month's movements of the jar, page by page, with the month's sums and balances. */
/** A month's jar movements' query, also used to load it ahead for offline use (web ADR 0022). */
export const savingsMovementsQuery = (month: Month) =>
  infiniteQueryOptions({
    queryKey: [...savingsKey, 'movements', month],
    initialPageParam: 1,
    getNextPageParam: nextPage,
    queryFn: ({ pageParam, signal }) =>
      apiCall(
        api.GET('/api/expenses/savings', {
          params: {
            query: { ...month, page: pageParam, pageSize: PAGE_SIZE },
          },
          signal,
        }),
      ),
  });

export function useSavingsMovements(month: Month) {
  return useInfiniteQuery(savingsMovementsQuery(month));
}

/** The query of `useSavingsOverview`, also used to load it ahead for offline use (web ADR 0022). */
export const savingsOverviewQuery = () =>
  queryOptions({
    queryKey: [...savingsKey, 'months'],
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/expenses/savings/months', { signal })),
  });

/** The jar's balance today and its movement month by month, newest first. */
export function useSavingsOverview() {
  return useQuery({ ...savingsOverviewQuery() });
}
