import { useMutation } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/client/api';
import { isStale } from './failures-api';
import { ifMatch, useTodosRefresh, type EditedTask } from './task-writes-api';

export type TodoCategoryRequest = Schema<'TodoCategoryRequest'>;

/**
 * Adds a category or renames one (with its version). A create takes no Idempotency-Key: a repeat hits the unique name
 * and answers 409 (backend design). Every category write refreshes the tasks too, which show the category's name.
 */
export function useSaveCategory() {
  const refresh = useTodosRefresh();
  return useMutation({
    mutationFn: ({
      body,
      edit,
    }: {
      body: TodoCategoryRequest;
      edit?: EditedTask;
    }) =>
      edit
        ? apiCall(
            api.PUT('/api/todos/categories/{id}', {
              params: ifMatch(edit),
              body,
            }),
          )
        : apiCall(api.POST('/api/todos/categories', { body })),
    onSuccess: () => void refresh(),
    onError: (error) => {
      if (isStale(error)) void refresh();
    },
  });
}

/** Deletes a category; its tasks stay, without one (R-008). */
export function useDeleteCategory() {
  const refresh = useTodosRefresh();
  return useMutation({
    mutationFn: (category: EditedTask) =>
      apiCall(
        api.DELETE('/api/todos/categories/{id}', {
          params: ifMatch(category),
        }),
      ),
    onSuccess: () => void refresh(),
    onError: (error) => {
      if (isStale(error)) void refresh();
    },
  });
}
