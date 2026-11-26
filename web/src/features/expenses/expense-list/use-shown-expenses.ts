import { useExpenses, type Payer } from '../data/expenses-api';
import { payerLabels } from '../data/labels';
import { monthDays } from '../data/month';
import { defaultDateIn } from '../entry-editing/entry-fields';
import { useClearFilters, useFilterParam } from '../url-state/use-filter-param';
import { useMonthParam } from '../url-state/use-month-param';
import { periodError } from '../url-state/period';
import { usePeriodParam } from '../url-state/use-period-param';

const payers = Object.keys(payerLabels) as Payer[];

/**
 * Which expenses «Расходы» shows, all from the URL: the open month, or a period of days when «Период» is on, with the
 * category and «Кто» filters. A wrong period loads nothing and says why (`periodProblem`).
 */
export function useShownExpenses() {
  const [month] = useMonthParam();
  const periodParam = usePeriodParam();
  const [categoryId, setCategoryId] = useFilterParam<string>('category');
  const [payer, setPayer] = useFilterParam<Payer>('payer', payers);
  const clearFilters = useClearFilters();
  const periodProblem = periodParam.active
    ? periodError(periodParam.period)
    : undefined;
  const period =
    periodParam.active && !periodProblem ? periodParam.period : undefined;

  return {
    month,
    periodParam,
    periodProblem,
    query: useExpenses(period ?? month, { categoryId, payer }, !periodProblem),
    filters: {
      categoryId,
      payer,
      onCategoryChange: setCategoryId,
      onPayerChange: setPayer,
    },
    isFiltered: Boolean(categoryId || payer),
    clearFilters,
    listLabel: periodParam.active ? 'Расходы за период' : 'Расходы за месяц',
    /** The date a new expense starts with: today when it is in the shown days, else their first day. */
    newDate: defaultDateIn(period ?? monthDays(month)),
  };
}
