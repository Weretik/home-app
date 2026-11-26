import type { ShoppingItem } from '../data/shopping-api';

export interface HistoryDay {
  /** The local date, «2026-11-26». */
  key: string;
  /** «Сегодня», «Вчера» or «вторник, 24 ноября». */
  label: string;
  items: ShoppingItem[];
}

const dayName = new Intl.DateTimeFormat('ru-RU', {
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
  if (localKey(date) === localKey(now)) return 'Сегодня';
  if (localKey(date) === localKey(yesterday)) return 'Вчера';
  const text = dayName.format(date);
  return date.getFullYear() === now.getFullYear()
    ? text
    : `${text} ${date.getFullYear()}`;
}

/**
 * History rows by the local day they were bought, newest day first, the API's order kept inside a day (newest
 * first). Rows without a time (should not happen) go under «Без даты» at the end.
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
    sorted.push({ key: 'undated', label: 'Без даты', items: undated });
  return sorted;
}
