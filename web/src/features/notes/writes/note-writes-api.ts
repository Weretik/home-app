import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useRef } from 'react';
import { apiCall, newIdempotencyKey, type Schema } from '../../../api-client';
import { api } from '../../../platform/client/api';
import { notesKey } from '../data/notes-api';
import { isStale } from './failures-api';

export type NoteRequest = Schema<'NoteRequest'>;

/** The note (or category) an edit starts from: its id and the version the screen showed (sent in If-Match). */
export interface Edited {
  id: string;
  version: number;
}

export const ifMatch = (edited: Edited) => ({
  path: { id: edited.id },
  header: { 'If-Match': String(edited.version) },
});

export type SaveNoteInput =
  | { body: NoteRequest; idempotencyKey: string; edit?: undefined }
  | { body: NoteRequest; edit: Edited };

/** Refreshes everything the «Про нас» screens show; also after a stale write, so the screen catches up. */
export function useNotesRefresh() {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: notesKey });
}

/** Adds a note (with an Idempotency-Key) or changes one (with its version in If-Match). */
export function useSaveNote() {
  const refresh = useNotesRefresh();
  return useMutation({
    mutationFn: (input: SaveNoteInput) =>
      input.edit
        ? apiCall(
            api.PUT('/api/notes/items/{id}', {
              params: ifMatch(input.edit),
              body: input.body,
            }),
          )
        : apiCall(
            api.POST('/api/notes/items', {
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

/** Deletes a note for good. */
export function useDeleteNote() {
  const refresh = useNotesRefresh();
  return useMutation({
    mutationFn: (note: Edited) =>
      apiCall(api.DELETE('/api/notes/items/{id}', { params: ifMatch(note) })),
    onSuccess: () => void refresh(),
    onError: (error) => {
      if (isStale(error)) void refresh();
    },
  });
}

/**
 * The Idempotency-Key of one opened form or one quick-add text (docs/rules.md, "Writes"): the same key while the same
 * data is retried, a new one when the data changes, and a new one after a success (`renew`), so adding the same text
 * twice on purpose makes two notes.
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
