import { useInfiniteQuery, useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/api';
import type { Month } from './month';

export type ExpenseItem = Schema<'ExpenseItem'>;
export type IncomeItem = Schema<'IncomeItem'>;
export type CategoryItem = Schema<'CategoryItem'>;
export type Payer = Schema<'Payer'>;
export type Person = Schema<'Person'>;
export type PaymentMethod = Schema<'PaymentMethod'>;

/** Rows per request; the API allows up to 100. */
export const PAGE_SIZE = 50;

export interface ExpenseFilter {
  categoryId?: string;
  payer?: Payer;
}

export interface IncomeFilter {
  person?: Person;
}

export interface Paged {
  page: number;
  pageSize: number;
  totalCount: number;
}

/** The next page number of a paged list, or undefined after the last page. */
export const nextPage = (last: Paged) =>
  last.page * last.pageSize < last.totalCount ? last.page + 1 : undefined;

/** The days an expense list shows: a month, or a period from–to («YYYY-MM-DD», both included). */
export type ExpenseDays = Month | { from: string; to: string };

/**
 * The expenses of a month or a period, loaded page by page («Показать ещё»). `enabled` false sends nothing (a
 * period the page found invalid).
 */
export function useExpenses(
  days: ExpenseDays,
  filter: ExpenseFilter,
  enabled = true,
) {
  return useInfiniteQuery({
    queryKey: ['expenses', 'list', days, filter],
    enabled,
    initialPageParam: 1,
    getNextPageParam: nextPage,
    queryFn: ({ pageParam, signal }) =>
      apiCall(
        api.GET('/api/expenses', {
          params: {
            query: {
              ...days,
              ...filter,
              page: pageParam,
              pageSize: PAGE_SIZE,
            },
          },
          signal,
        }),
      ),
  });
}

/** One month's incomes, loaded page by page. */
export function useIncomes(month: Month, filter: IncomeFilter) {
  return useInfiniteQuery({
    queryKey: ['expenses', 'incomes', month, filter],
    initialPageParam: 1,
    getNextPageParam: nextPage,
    queryFn: ({ pageParam, signal }) =>
      apiCall(
        api.GET('/api/expenses/incomes', {
          params: {
            query: {
              ...month,
              ...filter,
              page: pageParam,
              pageSize: PAGE_SIZE,
            },
          },
          signal,
        }),
      ),
  });
}

/** Expense categories for the filter; they change rarely. */
export function useExpenseCategories() {
  return useQuery({
    queryKey: ['expenses', 'categories'],
    staleTime: 5 * 60_000,
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/expenses/categories', { signal })),
  });
}
