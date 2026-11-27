import type { Assignee, TaskQuery, TaskView } from '../data/todos-api';
import { dateQuery, type DateFilter } from './date-filter';

/** «Кому» in the URL (`?who=`): lower-case words, so a link reads well. */
export type Who = 'maria' | 'vitaliy' | 'shared';

export const whoValues: readonly Who[] = ['maria', 'vitaliy', 'shared'];

export const whoLabels: Record<Who, string> = {
  maria: 'Маша',
  vitaliy: 'Віталій',
  shared: 'Спільне',
};

/** The filters of a task list as the URL keeps them (FR-002). `category` is an id or `none`. */
export interface TodoFilters extends DateFilter {
  who?: Who;
  category?: string;
  q?: string;
}

const whoAssignee: Record<Who, Assignee> = {
  maria: 'Maria',
  vitaliy: 'Vitaliy',
  shared: 'Shared',
};

/**
 * The API query of the filters. «Маша» and «Віталій» also bring the shared tasks, as the old sheet's per-person tabs
 * did; «Спільне» brings only the shared ones. The date filter reads `today` (Kyiv) and the list it is on.
 */
export function toTaskQuery(
  filters: TodoFilters,
  view: TaskView = 'active',
  today = '',
): TaskQuery {
  const { who, category, q } = filters;
  const query: TaskQuery = today ? dateQuery(view, filters, today) : {};
  if (who)
    query.assignee =
      who === 'shared' ? ['Shared'] : [whoAssignee[who], 'Shared'];
  if (category === 'none') query.withoutCategory = true;
  else if (category) query.categoryId = category;
  const search = q?.trim();
  if (search) query.search = search;
  return query;
}

/** The assignee of a quick-added task: the chosen «Кому», or «Спільне» when the list shows everyone (FR-003). */
export const quickAddAssignee = (who: Who | undefined): Assignee =>
  who ? whoAssignee[who] : 'Shared';

export const isFiltered = (filters: TodoFilters) =>
  Boolean(filters.who || filters.category || filters.q?.trim() || filters.when);
