import { useMutation } from '@tanstack/react-query';
import { apiCall } from '../../../api-client';
import { api } from '../../../platform/api';
import { useTodosRefresh } from './task-writes-api';

const path = (taskId: string, stepId: string) => ({
  path: { id: taskId, stepId },
});

/**
 * The steps of an existing task, saved at once (FR-005). They need no version (R-010), but they change the task's,
 * so each write waits for the refresh: the dialog then holds the new version before «Зберегти» can be pressed.
 */
export function useStepWrites(taskId: string) {
  const refresh = useTodosRefresh();
  const options = { onSettled: refresh };

  const add = useMutation({
    mutationFn: (title: string) =>
      apiCall(
        api.POST('/api/todos/tasks/{id}/steps', {
          params: { path: { id: taskId } },
          body: { title },
        }),
      ),
    ...options,
  });
  const update = useMutation({
    mutationFn: ({
      stepId,
      title,
      isDone,
    }: {
      stepId: string;
      title: string;
      isDone: boolean;
    }) =>
      apiCall(
        api.PUT('/api/todos/tasks/{id}/steps/{stepId}', {
          params: path(taskId, stepId),
          body: { title, isDone },
        }),
      ),
    ...options,
  });
  const remove = useMutation({
    mutationFn: (stepId: string) =>
      apiCall(
        api.DELETE('/api/todos/tasks/{id}/steps/{stepId}', {
          params: path(taskId, stepId),
        }),
      ),
    ...options,
  });

  return { add, update, remove };
}
