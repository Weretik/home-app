import { useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/api';

export type BudgetView = Schema<'BudgetView'>;
export type SpendingLimitItem = Schema<'SpendingLimitItem'>;
export type LimitPeriod = Schema<'LimitPeriod'>;

/** The budget's query key; every budget write refreshes it. */
export const budgetKey = ['expenses', 'budget'] as const;

/** The budget the settings page edits: the monthly limit, the savings goal (0 = none) and the spending limits. */
export function useBudget() {
  return useQuery({
    queryKey: budgetKey,
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/expenses/budget', { signal })),
  });
}
