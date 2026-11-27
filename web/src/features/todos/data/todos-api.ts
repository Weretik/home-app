import {
  infiniteQueryOptions,
  useInfiniteQuery,
  useQuery,
} from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/client/api';
import { moduleKeys } from '../../../platform/client/query-client';

export type TodoTaskItem = Schema<'TodoTaskItem'>;
export type TodoTaskList = Schema<'TodoTaskList'>;
export type TaskStepItem = Schema<'TaskStepItem'>;
export type Assignee = Schema<'Assignee'>;
export type Author = Schema<'Person'>;
export type TodoStatus = Schema<'TodoStatus'>;
export type TodoPriority = Schema<'TodoPriority'>;

/** Rows per request; the API allows up to 100. */
export const PAGE_SIZE = 50;

/**
 * Everything the Todos screens read sits under the module prefix (`moduleKeys.todos`), so one invalidation refreshes
 * them all, the home summary too (web ADR 0009).
 */
export const todosKey = moduleKeys.todos;
export const tasksKey = [...moduleKeys.todos, 'tasks'] as const;

export type TaskView = 'active' | 'archive';

/** The list query the filters turn into (url-state/filters.ts). */
export interface TaskQuery {
  assignee?: Assignee[];
  categoryId?: string;
  withoutCategory?: boolean;
  search?: string;
  dueFrom?: string;
  dueTo?: string;
  withoutDueDate?: boolean;
  completedFrom?: string;
  completedTo?: string;
}

/** The next page number of a paged list, or undefined after the last page. */
export const nextPage = (last: TodoTaskList) =>
  last.page * last.pageSize < last.totalCount ? last.page + 1 : undefined;

/**
 * Active tasks or the archive, page by page («Показати ще»), in the API's order (R-012); also loaded ahead for
 * offline use (web ADR 0022).
 */
export const tasksQuery = (view: TaskView, query: TaskQuery) =>
  infiniteQueryOptions({
    queryKey: [...tasksKey, view, query],
    initialPageParam: 1,
    getNextPageParam: nextPage,
    queryFn: ({ pageParam, signal }) =>
      apiCall(
        api.GET('/api/todos/tasks', {
          params: {
            query: { view, ...query, page: pageParam, pageSize: PAGE_SIZE },
          },
          signal,
        }),
      ),
  });

export function useTasks(view: TaskView, query: TaskQuery) {
  return useInfiniteQuery(tasksQuery(view, query));
}

/** One task fresh from the API; used for the undo of a done tick, which needs the version after it. */
export const fetchTask = (id: string) =>
  apiCall(api.GET('/api/todos/tasks/{id}', { params: { path: { id } } }));

/**
 * The task an open dialog shows: starts from the list's row and follows every refresh, so steps saved at once and
 * the version they change are always the current ones.
 */
export function useTask(item: TodoTaskItem | undefined) {
  return useQuery({
    queryKey: [...moduleKeys.todos, 'task', item?.id],
    enabled: Boolean(item),
    initialData: item,
    staleTime: 30_000,
    queryFn: ({ signal }) =>
      apiCall(
        api.GET('/api/todos/tasks/{id}', {
          params: { path: { id: item!.id } },
          signal,
        }),
      ),
  });
}
