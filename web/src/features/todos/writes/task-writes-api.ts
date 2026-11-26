import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useRef } from 'react';
import { apiCall, newIdempotencyKey, type Schema } from '../../../api-client';
import { api } from '../../../platform/api';
import { todosKey } from '../data/todos-api';
import { isStale } from './failures-api';

export type TodoTaskRequest = Schema<'TodoTaskRequest'>;
export type TodoTaskUpdate = Schema<'TodoTaskUpdateRequest'>;

/** The task an edit starts from: its id and the version the screen showed (sent in If-Match). */
export interface EditedTask {
  id: string;
  version: number;
}

export const ifMatch = (task: EditedTask) => ({
  path: { id: task.id },
  header: { 'If-Match': String(task.version) },
});

export type SaveTaskInput =
  | { body: TodoTaskRequest; idempotencyKey: string; edit?: undefined }
  | { body: TodoTaskUpdate; edit: EditedTask };

/** Refreshes everything the Todos screens show; also after a stale write, so the screen catches up. */
export function useTodosRefresh() {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: todosKey });
}

/** Adds a task (with an Idempotency-Key and its first steps) or changes one (with its version). */
export function useSaveTask() {
  const refresh = useTodosRefresh();
  return useMutation({
    mutationFn: (input: SaveTaskInput) =>
      input.edit
        ? apiCall(
            api.PUT('/api/todos/tasks/{id}', {
              params: ifMatch(input.edit),
              body: input.body,
            }),
          )
        : apiCall(
            api.POST('/api/todos/tasks', {
              params: { header: { 'Idempotency-Key': input.idempotencyKey } },
              body: input.body,
            }),
          ),
    onSuccess: () => void refresh(),
    onError: (error) => {
      if (isStale(error)) void refresh();
    },
  });
}

/** Deletes a task for good. */
export function useDeleteTask() {
  const refresh = useTodosRefresh();
  return useMutation({
    mutationFn: (task: EditedTask) =>
      apiCall(api.DELETE('/api/todos/tasks/{id}', { params: ifMatch(task) })),
    onSuccess: () => void refresh(),
    onError: (error) => {
      if (isStale(error)) void refresh();
    },
  });
}

/**
 * The Idempotency-Key of one opened form or one quick-add text (docs/rules.md, "Writes"): the same key while the same
 * data is retried, a new one when the data changes, and a new one after a success (`renew`), so adding the same title
 * twice on purpose makes two tasks.
 */
export function useIdempotencyKey() {
  const last = useRef<{ body: string; key: string } | null>(null);
  return {
    keyFor: (body: unknown): string => {
      const text = JSON.stringify(body);
      if (last.current?.body !== text)
        last.current = { body: text, key: newIdempotencyKey() };
      return last.current.key;
    },
    renew: () => {
      last.current = null;
    },
  };
}
