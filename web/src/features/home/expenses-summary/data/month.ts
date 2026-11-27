import { locale } from '../../../../platform/client/locale';

/** A calendar month the summary shows. `month` is 1–12. */
export interface Month {
  year: number;
  month: number;
}

const monthName = new Intl.DateTimeFormat(locale, { month: 'long' });

export function currentMonth(today: Date = new Date()): Month {
  return { year: today.getFullYear(), month: today.getMonth() + 1 };
}

/** The month `delta` months away: -1 is the previous one, 1 the next. */
export function shiftMonth({ year, month }: Month, delta: number): Month {
  const index = year * 12 + (month - 1) + delta;
  return { year: Math.floor(index / 12), month: (index % 12) + 1 };
}

/** Reads a month from URL values; anything missing or out of range gives `fallback`. */
export function parseMonth(
  year: string | null,
  month: string | null,
  fallback: Month,
): Month {
  const y = Number(year);
  const m = Number(month);
  const valid =
    Number.isInteger(y) &&
    Number.isInteger(m) &&
    y >= 2000 &&
    y <= 2100 &&
    m >= 1 &&
    m <= 12;
  return valid ? { year: y, month: m } : fallback;
}

/** «Жовтень 2026». */
export function formatMonth({ year, month }: Month): string {
  const name = monthName.format(new Date(year, month - 1, 1));
  return `${name.charAt(0).toUpperCase()}${name.slice(1)} ${year}`;
}
