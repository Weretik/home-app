import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ApiError, apiCall, type Schema } from '../../../../api-client';
import { api } from '../../../../platform/api';
import { moduleKeys } from '../../../../platform/query-keys';

export type TodoSummary = Schema<'TodoSummary'>;
export type AssigneeCounts = Schema<'AssigneeCounts'>;
export type FocusTask = Schema<'TodoTaskItem'>;
export type Assignee = Schema<'Assignee'>;
export type TodoPriority = Schema<'TodoPriority'>;

/**
 * The family's tasks at a glance (`GET /api/todos/summary`). The key sits under the module prefix `moduleKeys.todos`, so every write of the
 * Todos section refreshes the summary too.
 */
export function useTodoSummary() {
  return useQuery({
    queryKey: [...moduleKeys.todos, 'summary'],
    queryFn: ({ signal }) => apiCall(api.GET('/api/todos/summary', { signal })),
  });
}

/** Ticks a task of «На сегодня» done in place (FR-008), with its version; then everything under the module prefix reloads. */
export function useCompleteFocusTask() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (task: Pick<FocusTask, 'id' | 'version'>) =>
      apiCall(
        api.PUT('/api/todos/tasks/{id}/status', {
          params: {
            path: { id: task.id },
            header: { 'If-Match': String(task.version) },
          },
          body: { status: 'Done' },
        }),
      ),
    onSettled: () =>
      queryClient.invalidateQueries({ queryKey: moduleKeys.todos }),
  });
}

/** Why a tick in «На сегодня» failed, in Russian (the API's own messages are English). The summary reloads after it. */
export function describeTickError(error: Error): string {
  if (!(error instanceof ApiError))
    return 'Нет связи с сервером. Проверьте интернет и попробуйте снова.';
  if (error.status === 404 || error.code === 'todos.changed_meanwhile')
    return 'Задачу изменили в другом месте — сводка обновлена.';
  if (error.status === 429)
    return 'Слишком много изменений подряд. Подождите минуту и повторите.';
  return `Не удалось отметить задачу: ${error.message}`;
}
