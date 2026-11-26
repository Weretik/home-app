import { useInfiniteQuery, useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/api';
import { moduleKeys } from '../../../platform/query-keys';
import { nextPage, PAGE_SIZE } from './expenses-api';
import type { Month } from './month';

export type SavingsMovementItem = Schema<'SavingsMovementItem'>;
export type SavingsMonthSummary = Schema<'SavingsMonthSummary'>;
export type SavingsDirection = Schema<'SavingsDirection'>;

/** Every savings query starts with this key, under the module's prefix. */
const savingsKey = [...moduleKeys.expenses, 'savings'] as const;

/** One month's movements of the jar, page by page, with the month's sums and balances. */
export function useSavingsMovements(month: Month) {
  return useInfiniteQuery({
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
}

/** The jar's balance today and its movement month by month, newest first. */
export function useSavingsOverview() {
  return useQuery({
    queryKey: [...savingsKey, 'months'],
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/expenses/savings/months', { signal })),
  });
}
