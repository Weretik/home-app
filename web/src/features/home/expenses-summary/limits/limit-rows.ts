import { limitName } from '../data/labels';
import { formatExact, percentOf } from '../data/money';
import type { MonthSummary } from '../data/summary-api';

/** One row of the «Лимиты» table. `limit` and `left` are null when there is nothing to compare with. */
export interface LimitRow {
  key: string;
  name: string;
  spent: number;
  limit: number | null;
  left: number | null;
  percent: number | null;
  perDay: number | null;
  /** Overspend carried in from earlier periods; it counts as spent. */
  carriedIn: number;
  /** Overspend that moves on to the next period. */
  carriesOut: number;
  /** Planned expenses («Прогноз») of the limit's categories in its period. */
  planned: number;
  isTotal: boolean;
}

/** How full a limit is: under 80 % fine, up to 100 % close, over 100 % spent past it. */
export type Fill = 'success' | 'warning' | 'error';

export const fillOf = (percent: number): Fill =>
  percent > 100 ? 'error' : percent >= 80 ? 'warning' : 'success';

/** The month's expenses that count against the limits: all of them but those marked «Не в лимиты». */
export const limitCounted = (summary: MonthSummary) =>
  Math.round((summary.expenses - summary.outsideLimitsAmount) * 100) / 100;

/** How full the monthly limit is, counting the overspend carried in, as the «ИТОГО» row and the «Расходы» card show it. */
export const totalPercent = (summary: MonthSummary) =>
  percentOf(limitCounted(summary) + summary.carriedIn, summary.monthlyLimit);

/**
 * The limits of a month, then «ИТОГО»: the expenses that count against the limits (not those marked «Не в лимиты»),
 * plus the overspend carried in, against the monthly limit, with what may still be spent per day (never below zero)
 * while the month runs, the way the API counts it for each limit.
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
    carriedIn: limit.carriedIn,
    carriesOut: limit.carriesOut,
    planned: limit.planned,
    isTotal: false,
  }));

  const hasLimit = summary.monthlyLimit > 0;
  const spent = limitCounted(summary);
  const left =
    Math.round((summary.monthlyLimit - spent - summary.carriedIn) * 100) / 100;
  rows.push({
    key: 'total',
    name: 'ИТОГО',
    spent,
    limit: hasLimit ? summary.monthlyLimit : null,
    left: hasLimit ? left : null,
    percent: hasLimit ? totalPercent(summary) : null,
    perDay:
      hasLimit && summary.daysLeft
        ? Math.max(0, Math.round((left / summary.daysLeft) * 100) / 100)
        : null,
    carriedIn: summary.carriedIn,
    carriesOut: summary.carriesOut,
    planned: summary.forecast.planned,
    isTotal: true,
  });
  return rows;
}

/** The notes after a row's figures, only those that are not zero: «перенос …», «→ след. мес. …», «план …». */
export function limitNotes(
  row: Pick<LimitRow, 'carriedIn' | 'carriesOut' | 'planned'>,
): { key: string; text: string }[] {
  const notes = [
    { key: 'in', value: row.carriedIn, label: 'перенос' },
    { key: 'out', value: row.carriesOut, label: '→ след. мес.' },
    { key: 'planned', value: row.planned, label: 'план' },
  ];
  return notes
    .filter((note) => note.value > 0)
    .map((note) => ({
      key: note.key,
      text: `${note.label} ${formatExact(note.value)}`,
    }));
}

/** The accessible name of the limits table and list. */
export const limitsLabel = 'Лимиты месяца';
/** Money columns: never wrapped, digits aligned. */
export const numberSx = {
  whiteSpace: 'nowrap',
  fontVariantNumeric: 'tabular-nums',
} as const;
/** A negative remainder (overspent) is shown in the error colour. */
export const leftColor = (left: number | null) =>
  left !== null && left < 0 ? 'error.main' : 'inherit';
/** A dash where a row has no value (a category without a limit). */
export const orDash = (value: number | null, format: (v: number) => string) =>
  value === null ? '—' : format(value);
