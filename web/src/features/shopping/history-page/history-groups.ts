import { locale } from '../../../platform/client/locale';
import type { ShoppingItem } from '../data/shopping-api';

export interface HistoryDay {
  /** The local date, «2026-11-26». */
  key: string;
  /** «Сьогодні», «Вчора» or «вівторок, 24 листопада». */
  label: string;
  items: ShoppingItem[];
}

const dayName = new Intl.DateTimeFormat(locale, {
  weekday: 'long',
  day: 'numeric',
  month: 'long',
});

const localKey = (date: Date) =>
  `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;

/** The heading of a day relative to today. */
export function dayLabel(date: Date, now: Date = new Date()): string {
  const yesterday = new Date(now);
  yesterday.setDate(now.getDate() - 1);
  if (localKey(date) === localKey(now)) return 'Сьогодні';
  if (localKey(date) === localKey(yesterday)) return 'Вчора';
  const text = dayName.format(date);
  return date.getFullYear() === now.getFullYear()
    ? text
    : `${text} ${date.getFullYear()}`;
}

/**
 * History rows by the local day they were bought, newest day first, the API's order kept inside a day (newest
 * first). Rows without a time (should not happen) go under «Без дати» at the end.
 */
export function groupByDay(
  items: ShoppingItem[],
  now: Date = new Date(),
): HistoryDay[] {
  const days = new Map<string, HistoryDay>();
  const undated: ShoppingItem[] = [];
  for (const item of items) {
    if (!item.boughtAtUtc) {
      undated.push(item);
      continue;
    }
    const date = new Date(item.boughtAtUtc);
    const key = localKey(date);
    const day = days.get(key) ?? { key, label: dayLabel(date, now), items: [] };
    day.items.push(item);
    days.set(key, day);
  }
  const sorted = [...days.values()].sort((a, b) => b.key.localeCompare(a.key));
  if (undated.length > 0)
    sorted.push({ key: 'undated', label: 'Без дати', items: undated });
  return sorted;
}

export interface HistoryMonth {
  /** The local month, «2026-11»; «undated» for rows without a time. */
  key: string;
  /** «Листопад 2026» or «Без дати». */
  label: string;
  /** Loaded rows of the month. */
  count: number;
  /** More pages may hold more rows of this month (only the last loaded month). */
  partial: boolean;
  days: HistoryDay[];
}

const monthName = new Intl.DateTimeFormat(locale, { month: 'long' });

/** «Листопад 2026» for the month of a «2026-11-26» day key. */
export function monthLabel(dayKey: string): string {
  const [year, month] = dayKey.split('-').map(Number);
  const name = monthName.format(new Date(year!, month! - 1, 1));
  return `${name.charAt(0).toUpperCase()}${name.slice(1)} ${year}`;
}

/**
 * The day groups under their local month, newest month first (FR-301). A day is always inside one month, so the months
 * are built from `groupByDay`. When `mayContinue` (more pages exist), the last month is `partial`: the next page can
 * hold more of it, and it continues in place.
 */
export function groupByMonth(
  items: ShoppingItem[],
  now: Date = new Date(),
  mayContinue = false,
): HistoryMonth[] {
  const months: HistoryMonth[] = [];
  for (const day of groupByDay(items, now)) {
    const key = day.key === 'undated' ? 'undated' : day.key.slice(0, 7);
    let month = months.at(-1);
    if (month?.key !== key) {
      month = {
        key,
        label: key === 'undated' ? 'Без дати' : monthLabel(day.key),
        count: 0,
        partial: false,
        days: [],
      };
      months.push(month);
    }
    month.days.push(day);
    month.count += day.items.length;
  }
  const last = months.filter((month) => month.key !== 'undated').at(-1);
  if (last && mayContinue) last.partial = true;
  return months;
}

/** «1 покупка», «3 покупки», «38 покупок»; «50+ покупок» while the month may continue on the next page (FR-302). */
export function purchaseCount(count: number, partial = false): string {
  if (partial) return `${count}+ покупок`;
  const forms = { one: 'покупка', few: 'покупки', many: 'покупок' } as const;
  const rule = new Intl.PluralRules(locale).select(count);
  return `${count} ${forms[rule as keyof typeof forms] ?? forms.many}`;
}

/** The heading of a month: «Листопад 2026 · 38 покупок». */
export const monthHeading = (month: HistoryMonth) =>
  `${month.label} · ${purchaseCount(month.count, month.partial)}`;
