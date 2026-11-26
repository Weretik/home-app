/**
 * The day headings of the lists (FR-001, FR-006): the active tasks by due day («Просрочено», «Сегодня», «Завтра»,
 * «Пн, 30 ноября» …, «Без срока») and the archive by the Kyiv day of completion («Сегодня», «Вчера», «Пт, 20 ноября»).
 * Items keep the API's order inside a group.
 */
import { addDays, kyivDayOf } from './due-date';

export interface DayGroup<T> {
  /** A stable key: a day «YYYY-MM-DD», `overdue` or `none`. */
  key: string;
  label: string;
  /** The heading is red: the tasks are late. */
  overdue: boolean;
  items: T[];
}

const weekdays = ['Вс', 'Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб'] as const;
const months = [
  'января',
  'февраля',
  'марта',
  'апреля',
  'мая',
  'июня',
  'июля',
  'августа',
  'сентября',
  'октября',
  'ноября',
  'декабря',
] as const;

/** «Пн, 30 ноября», with the year outside the year of `today`: «Пн, 4 января 2027». */
export function dayHeading(day: string, today: string): string {
  const [year = 2000, month = 1, date = 1] = day.split('-').map(Number);
  const weekday =
    weekdays[new Date(Date.UTC(year, month - 1, date)).getUTCDay()];
  const sameYear = day.slice(0, 4) === today.slice(0, 4);
  return `${weekday}, ${date} ${months[month - 1]}${sameYear ? '' : ` ${year}`}`;
}

/** Groups by a key in the order the keys first appear. */
function groupBy<T>(
  items: readonly T[],
  keyOf: (item: T) => string,
): Map<string, T[]> {
  const groups = new Map<string, T[]>();
  for (const item of items) {
    const key = keyOf(item);
    const group = groups.get(key);
    if (group) group.push(item);
    else groups.set(key, [item]);
  }
  return groups;
}

/** The active tasks: overdue first, then every due day in date order, the tasks without a due date last. */
export function groupByDueDay<T extends { dueDate?: string | null }>(
  items: readonly T[],
  today: string,
): DayGroup<T>[] {
  const tomorrow = addDays(today, 1);
  const keyOf = (item: T) =>
    !item.dueDate ? 'none' : item.dueDate < today ? 'overdue' : item.dueDate;
  const rank = (key: string) =>
    key === 'overdue' ? '0' : key === 'none' ? '2' : `1${key}`;

  return [...groupBy(items, keyOf)]
    .sort(([a], [b]) => rank(a).localeCompare(rank(b)))
    .map(([key, group]) => ({
      key,
      label:
        key === 'overdue'
          ? 'Просрочено'
          : key === 'none'
            ? 'Без срока'
            : key === today
              ? 'Сегодня'
              : key === tomorrow
                ? 'Завтра'
                : dayHeading(key, today),
      overdue: key === 'overdue',
      items: group,
    }));
}

/** The archive: one group per Kyiv day of completion, newest first (the API's order). */
export function groupByCompletionDay<
  T extends { completedAtUtc?: string | null },
>(items: readonly T[], today: string): DayGroup<T>[] {
  const yesterday = addDays(today, -1);
  const keyOf = (item: T) =>
    item.completedAtUtc ? kyivDayOf(item.completedAtUtc) : 'none';

  return [...groupBy(items, keyOf)]
    .sort(([a], [b]) =>
      a === 'none' ? 1 : b === 'none' ? -1 : b.localeCompare(a),
    )
    .map(([key, group]) => ({
      key,
      label:
        key === 'none'
          ? 'Без даты'
          : key === today
            ? 'Сегодня'
            : key === yesterday
              ? 'Вчера'
              : dayHeading(key, today),
      overdue: false,
      items: group,
    }));
}
