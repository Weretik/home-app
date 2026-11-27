import { useQuery, queryOptions } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../../api-client';
import { api } from '../../../../platform/client/api';
import { moduleKeys } from '../../../../platform/client/query-client';
import type { Month } from './month';

export type MonthSummary = Schema<'MonthSummary'>;
export type LimitUsage = Schema<'LimitUsage'>;
export type Payer = Schema<'Payer'>;
export type Person = Schema<'Person'>;

/** The query of `useMonthSummary`, also used to load it ahead for offline use (web ADR 0022). */
export const monthSummaryQuery = (month: Month) =>
  queryOptions({
    queryKey: [...moduleKeys.expenses, 'summary', month],
    queryFn: ({ signal }) =>
      apiCall(
        api.GET('/api/expenses/summary', { params: { query: month }, signal }),
      ),
  });

/**
 * One month of the Expenses module at a glance (`GET /api/expenses/summary`). The key starts with the module's prefix
 * (`moduleKeys.expenses`), so every write to the module, made on any page of the `expenses` feature, refreshes it.
 */
export function useMonthSummary(month: Month) {
  return useQuery({ ...monthSummaryQuery(month) });
}
