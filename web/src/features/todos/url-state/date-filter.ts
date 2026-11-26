/**
 * The date filter of a list (FR-002, FR-006), kept in the URL as `?when=` (and `?from=&to=` for a period). On the
 * active list it reads the due date («Срок»); in the archive the Kyiv day of completion («Когда сделано»). «Неделя» and
 * «Месяц» on the active list keep the overdue tasks, since they still have to be done in that time.
 */
import { addDays } from '../data/due-date';
import type { TaskQuery, TaskView } from '../data/todos-api';

export type When =
  | 'overdue'
  | 'today'
  | 'tomorrow'
  | 'yesterday'
  | 'week'
  | 'month'
  | 'none'
  | 'period';

/** The choices of each list, in the order they are offered. */
export const whenOptions: Record<TaskView, readonly When[]> = {
  active: ['overdue', 'today', 'tomorrow', 'week', 'month', 'none', 'period'],
  archive: ['today', 'yesterday', 'week', 'month', 'period'],
};

export const whenLabels: Record<When, string> = {
  overdue: 'Просрочено',
  today: 'Сегодня',
  tomorrow: 'Завтра',
  yesterday: 'Вчера',
  week: 'Неделя',
  month: 'Месяц',
  none: 'Без срока',
  period: 'Период',
};

export const whenTitles: Record<TaskView, string> = {
  active: 'Срок',
  archive: 'Когда сделано',
};

export interface DateFilter {
  when?: When;
  /** «YYYY-MM-DD», only with `period`. */
  from?: string;
  to?: string;
}

const isDay = (value: string | undefined): value is string =>
  Boolean(value && /^\d{4}-\d{2}-\d{2}$/.test(value));

/** Whether the list offers the choice; another list's choice in the URL is ignored. */
export const whenFits = (view: TaskView, when: When | undefined) =>
  when !== undefined && whenOptions[view].includes(when);

/** The days of a choice, both included; an open side is undefined. */
function range(
  view: TaskView,
  { when, from, to }: DateFilter,
  today: string,
): { from?: string; to?: string; none?: boolean } {
  const ahead = view === 'active';
  switch (when) {
    case 'overdue':
      return { to: addDays(today, -1) };
    case 'today':
      return { from: today, to: today };
    case 'tomorrow':
      return { from: addDays(today, 1), to: addDays(today, 1) };
    case 'yesterday':
      return { from: addDays(today, -1), to: addDays(today, -1) };
    case 'week':
      return ahead
        ? { to: addDays(today, 6) }
        : { from: addDays(today, -6), to: today };
    case 'month':
      return ahead
        ? { to: addDays(today, 29) }
        : { from: addDays(today, -29), to: today };
    case 'none':
      return { none: true };
    case 'period': {
      const start = isDay(from) ? from : undefined;
      const end = isDay(to) ? to : undefined;
      // A period entered backwards still means the days between.
      return start && end && start > end
        ? { from: end, to: start }
        : { from: start, to: end };
    }
    default:
      return {};
  }
}

/** The API parameters of the date filter: due dates on the active list, completion days in the archive. */
export function dateQuery(
  view: TaskView,
  filter: DateFilter,
  today: string,
): TaskQuery {
  if (!whenFits(view, filter.when)) return {};
  const { from, to, none } = range(view, filter, today);
  const query: TaskQuery = {};
  if (view === 'archive') {
    if (from) query.completedFrom = from;
    if (to) query.completedTo = to;
    return query;
  }
  if (none) query.withoutDueDate = true;
  if (from) query.dueFrom = from;
  if (to) query.dueTo = to;
  return query;
}
