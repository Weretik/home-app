import { limitName } from './labels';
import { percentOf } from './money';
import type { MonthSummary } from './summary-api';

/** One row of the «Лимиты» table. `limit` and `left` are null when there is nothing to compare with. */
export interface LimitRow {
  key: string;
  name: string;
  spent: number;
  limit: number | null;
  left: number | null;
  percent: number | null;
  perDay: number | null;
  isTotal: boolean;
}

/** How full a limit is: under 80 % fine, up to 100 % close, over 100 % spent past it. */
export type Fill = 'success' | 'warning' | 'error';

export const fillOf = (percent: number): Fill =>
  percent > 100 ? 'error' : percent >= 80 ? 'warning' : 'success';

/**
 * The limits of a month, then «ИТОГО»: all expenses against the monthly limit, with what may still be spent per day
 * (never below zero) while the month runs, the way the API counts it for each limit.
 */
export function limitRows(summary: MonthSummary): LimitRow[] {
  const rows: LimitRow[] = summary.limits.map((limit) => ({
    key: limit.id,
    name: limitName(limit),
    spent: limit.spent,
    limit: limit.amount,
    left: limit.left,
    percent: limit.percent,
    perDay: limit.perDay,
    isTotal: false,
  }));

  const hasLimit = summary.monthlyLimit > 0;
  const left = summary.monthlyLimit - summary.expenses;
  rows.push({
    key: 'total',
    name: 'ИТОГО',
    spent: summary.expenses,
    limit: hasLimit ? summary.monthlyLimit : null,
    left: hasLimit ? left : null,
    percent: hasLimit
      ? percentOf(summary.expenses, summary.monthlyLimit)
      : null,
    perDay:
      hasLimit && summary.daysLeft
        ? Math.max(0, Math.round((left / summary.daysLeft) * 100) / 100)
        : null,
    isTotal: true,
  });
  return rows;
}
