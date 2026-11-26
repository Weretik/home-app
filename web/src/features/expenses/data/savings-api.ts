import { useInfiniteQuery, useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/api';
import { nextPage, PAGE_SIZE } from './expenses-api';
import type { Month } from './month';
import { ifMatch, useRemove, useSave, type Writer } from './writes-api';

export type SavingsMovementItem = Schema<'SavingsMovementItem'>;
export type SavingsMonthSummary = Schema<'SavingsMonthSummary'>;
export type SavingsDirection = Schema<'SavingsDirection'>;
export type SavingsMovementRequest = Schema<'SavingsMovementRequest'>;

/** Every savings query starts with this key, so a write refreshes the movements and the months together. */
export const savingsKey = ['expenses', 'savings'] as const;

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

const savingsWriter: Writer<SavingsMovementRequest> = {
  key: savingsKey,
  create: (body, key) =>
    apiCall(
      api.POST('/api/expenses/savings', {
        params: { header: { 'Idempotency-Key': key } },
        body,
      }),
    ),
  update: (entry, body) =>
    apiCall(
      api.PUT('/api/expenses/savings/{id}', { params: ifMatch(entry), body }),
    ),
  remove: (id) =>
    apiCall(
      api.DELETE('/api/expenses/savings/{id}', { params: { path: { id } } }),
    ),
};

/** Adds or changes a savings movement; refreshes the movements and the months. */
export const useSaveSavingsMovement = () => useSave(savingsWriter);
export const useDeleteSavingsMovement = () =>
  useRemove(savingsWriter as Writer<unknown>);
