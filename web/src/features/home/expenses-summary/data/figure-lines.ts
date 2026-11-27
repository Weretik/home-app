import { locale } from '../../../../platform/client/locale';
import { formatWhole } from './money';
import type { MonthSummary } from './summary-api';

/** A month with no incomes, expenses, jar movements or plans: the summary shows the empty state under the figures. */
export const isEmptyMonth = (
  summary: Pick<
    MonthSummary,
    'incomes' | 'expenses' | 'savingsNet' | 'forecast'
  >,
) =>
  summary.incomes === 0 &&
  summary.expenses === 0 &&
  summary.savingsNet === 0 &&
  summary.forecast.planned === 0;

const spendings = { one: 'витрата', few: 'витрати', many: 'витрат' } as const;

/** «1 витрата», «3 витрати», «12 витрат». */
export function formatSpendingCount(count: number): string {
  const rule = new Intl.PluralRules(locale).select(count);
  return `${count} ${spendings[rule as keyof typeof spendings] ?? spendings.many}`;
}

/**
 * «Поза лімітами: 2 витрати на 3 500 ₴»: the month's expenses marked «Не в ліміти». Null when there are none, so the
 * line is not shown at all.
 */
export function outsideLimitsLine(
  summary: Pick<MonthSummary, 'outsideLimitsCount' | 'outsideLimitsAmount'>,
): string | null {
  if (summary.outsideLimitsCount <= 0) return null;
  return `Поза лімітами: ${formatSpendingCount(summary.outsideLimitsCount)} на ${formatWhole(summary.outsideLimitsAmount)}`;
}

/** One line of the «Прогноз» card. `tone` marks a result that is good or bad. */
export interface ForecastLine {
  key: string;
  name: string;
  amount: number;
  tone?: 'success' | 'error';
}

/**
 * The «Прогноз» card: the expected expenses (spent plus planned), what is planned, the expected result of the month,
 * and, with a monthly limit, what is expected to stay of it.
 */
export function forecastLines(summary: MonthSummary): ForecastLine[] {
  const { forecast } = summary;
  const lines: ForecastLine[] = [
    {
      key: 'expected',
      name: 'Очікувані витрати',
      amount: forecast.expectedExpenses,
    },
    { key: 'planned', name: 'Заплановано', amount: forecast.planned },
    {
      key: 'result',
      name: 'Очікуваний підсумок місяця',
      amount: forecast.expectedResult,
      tone: forecast.expectedResult >= 0 ? 'success' : 'error',
    },
  ];
  if (summary.monthlyLimit > 0)
    lines.push({
      key: 'left',
      name: 'Залишиться від ліміту',
      amount: forecast.expectedTotalLeft,
      tone: forecast.expectedTotalLeft < 0 ? 'error' : undefined,
    });
  return lines;
}
