import { useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/api';
import { moduleKeys } from '../../../platform/query-client';

export type BudgetView = Schema<'BudgetView'>;
export type SpendingLimitItem = Schema<'SpendingLimitItem'>;
export type LimitPeriod = Schema<'LimitPeriod'>;
export type AmountStep = Schema<'AmountStep'>;

/** The budget's query key, under the module's prefix (every write refreshes it). */
const budgetKey = [...moduleKeys.expenses, 'budget'] as const;

/** The budget the settings page edits: the monthly limit, the savings goal (0 = none) and the spending limits. */
export function useBudget() {
  return useQuery({
    queryKey: budgetKey,
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/expenses/budget', { signal })),
  });
}
