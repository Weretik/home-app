import { useMutation } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/client/api';
import { isStale } from './failures-api';
import { ifMatch, useNotesRefresh, type Edited } from './note-writes-api';

export type NoteCategoryRequest = Schema<'NoteCategoryRequest'>;

/**
 * Adds a category or renames one (with its version). A create takes no Idempotency-Key: a repeat hits the unique name
 * and answers 409 (backend design). Every category write refreshes the notes too, which show the category's name.
 */
export function useSaveCategory() {
  const refresh = useNotesRefresh();
  return useMutation({
    mutationFn: ({
      body,
      edit,
    }: {
      body: NoteCategoryRequest;
      edit?: Edited;
    }) =>
      edit
        ? apiCall(
            api.PUT('/api/notes/categories/{id}', {
              params: ifMatch(edit),
              body,
            }),
          )
        : apiCall(api.POST('/api/notes/categories', { body })),
    onSuccess: () => void refresh(),
    onError: (error) => {
      if (isStale(error)) void refresh();
    },
  });
}

/** Deletes a category; its notes stay, without one (R-006). */
export function useDeleteCategory() {
  const refresh = useNotesRefresh();
  return useMutation({
    mutationFn: (category: Edited) =>
      apiCall(
        api.DELETE('/api/notes/categories/{id}', {
          params: ifMatch(category),
        }),
      ),
    onSuccess: () => void refresh(),
    onError: (error) => {
      if (isStale(error)) void refresh();
    },
  });
}
