import { useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../../api-client';
import { api } from '../../../../platform/api';
import type { Month } from './month';

export type MonthSummary = Schema<'MonthSummary'>;
export type LimitUsage = Schema<'LimitUsage'>;
export type Payer = Schema<'Payer'>;
export type Person = Schema<'Person'>;

/**
 * One month of the Expenses module at a glance (`GET /api/expenses/summary`). The key sits under `['expenses']`, so
 * invalidating everything the Expenses module shows refreshes the summary too.
 */
export function useMonthSummary(month: Month) {
  return useQuery({
    queryKey: ['expenses', 'summary', month],
    queryFn: ({ signal }) =>
      apiCall(
        api.GET('/api/expenses/summary', { params: { query: month }, signal }),
      ),
  });
}
