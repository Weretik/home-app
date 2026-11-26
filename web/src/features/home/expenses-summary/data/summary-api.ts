import { useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../../api-client';
import { api } from '../../../../platform/api';
import { moduleKeys } from '../../../../platform/query-keys';
import type { Month } from './month';

export type MonthSummary = Schema<'MonthSummary'>;
export type LimitUsage = Schema<'LimitUsage'>;
export type Payer = Schema<'Payer'>;
export type Person = Schema<'Person'>;

/**
 * One month of the Expenses module at a glance (`GET /api/expenses/summary`). The key starts with the module's prefix
 * (`moduleKeys.expenses`), so every write to the module, made on any page of the `expenses` feature, refreshes it.
 */
export function useMonthSummary(month: Month) {
  return useQuery({
    queryKey: [...moduleKeys.expenses, 'summary', month],
    queryFn: ({ signal }) =>
      apiCall(
        api.GET('/api/expenses/summary', { params: { query: month }, signal }),
      ),
  });
}
