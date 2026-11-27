import {
  useMutation,
  useQueryClient,
  type InfiniteData,
} from '@tanstack/react-query';
import { apiCall } from '../../../api-client';
import { api } from '../../../platform/api';
import {
  fetchTask,
  tasksKey,
  todosKey,
  type TodoStatus,
  type TodoTaskList,
} from '../data/todos-api';
import { ifMatch, type EditedTask } from './task-writes-api';

export interface StatusChange extends EditedTask {
  status: TodoStatus;
}

const putStatus = ({ status, ...task }: StatusChange) =>
  apiCall(
    api.PUT('/api/todos/tasks/{id}/status', {
      params: ifMatch(task),
      body: { status },
    }),
  );

type Pages = InfiniteData<TodoTaskList, number>;

const without = (id: string) => (data: Pages | undefined) =>
  data && {
    ...data,
    pages: data.pages.map((page) => ({
      ...page,
      items: page.items.filter((item) => item.id !== id),
      totalCount: page.items.some((item) => item.id === id)
        ? page.totalCount - 1
        : page.totalCount,
    })),
  };

/**
 * Moves a task to another list: done or cancelled from the active list, «Повернути» from the archive. Optimistic: the
 * row leaves its list at once and comes back if the write fails (the caller shows why). Everything reloads after.
 */
export function useMoveTask() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: putStatus,
    onMutate: async (change) => {
      await queryClient.cancelQueries({ queryKey: tasksKey });
      const before = queryClient.getQueriesData<Pages>({ queryKey: tasksKey });
      queryClient.setQueriesData<Pages>(
        { queryKey: tasksKey },
        without(change.id),
      );
      return { before };
    },
    onError: (_error, _change, context) => {
      for (const [key, data] of context?.before ?? [])
        queryClient.setQueryData(key, data);
    },
    onSettled: () => queryClient.invalidateQueries({ queryKey: todosKey }),
  });
}

/**
 * Puts a task back to the status it had before a tick («Скасувати» in the snackbar). The tick changed the version, so
 * the task is read again first.
 */
export function useRestoreStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, status }: { id: string; status: TodoStatus }) => {
      const task = await fetchTask(id);
      return putStatus({ id, version: task.version, status });
    },
    onSettled: () => queryClient.invalidateQueries({ queryKey: todosKey }),
  });
}
