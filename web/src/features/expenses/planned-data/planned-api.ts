import { useQuery, queryOptions } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/client/api';
import { moduleKeys } from '../../../platform/client/query-client';
import type { Month } from '../data/month';

export type PlannedExpenseItem = Schema<'PlannedExpenseItem'>;

/** The query of `usePlannedExpenses`, also used to load it ahead for offline use (web ADR 0022). */
export const plannedExpensesQuery = (month: Month) =>
  queryOptions({
    queryKey: [...moduleKeys.expenses, 'planned', month],
    queryFn: ({ signal }) =>
      apiCall(
        api.GET('/api/expenses/planned', { params: { query: month }, signal }),
      ),
  });

/** «Прогноз»: the expenses planned for a month, ordered by name, and their total («Заплановано»). */
export function usePlannedExpenses(month: Month) {
  return useQuery({ ...plannedExpensesQuery(month) });
}
