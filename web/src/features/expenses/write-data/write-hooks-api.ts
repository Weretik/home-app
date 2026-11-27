import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useRef } from 'react';
import { ApiError, newIdempotencyKey } from '../../../api-client';
import { moduleKeys } from '../../../platform/query-client';

/** The entry an edit starts from: its id and the version the list showed (sent in If-Match). */
export interface EditedEntry {
  id: string;
  version: number;
}

export interface SaveInput<Body> {
  body: Body;
  /** Set for an edit; a create leaves it out. */
  edit?: EditedEntry;
  /** Sent with a create (see {@link useIdempotencyKey}). */
  idempotencyKey: string;
}

/** How one kind of entry is added, changed and deleted. */
export interface Writer<Body> {
  create: (body: Body, idempotencyKey: string) => Promise<unknown>;
  update: (entry: EditedEntry, body: Body) => Promise<unknown>;
  remove: (id: string) => Promise<unknown>;
}

export const ifMatch = (entry: EditedEntry) => ({
  path: { id: entry.id },
  header: { 'If-Match': String(entry.version) },
});

/** The entry is no longer what the list shows (changed or deleted elsewhere), so the list must reload. */
export const isStale = (error: Error) =>
  error instanceof ApiError &&
  (error.status === 404 || error.code === 'expenses.changed_meanwhile');

/**
 * A write to the Expenses module. A success refreshes every query of the module (its lists, the budget, the plans and
 * the home summary share `moduleKeys.expenses`); a stale entry refreshes them too, so the page shows what is there now.
 */
function useWrite<Input>(send: (input: Input) => Promise<unknown>) {
  const queryClient = useQueryClient();
  const refresh = () =>
    void queryClient.invalidateQueries({ queryKey: moduleKeys.expenses });
  return useMutation({
    mutationFn: send,
    onSuccess: refresh,
    onError: (error) => {
      if (isStale(error)) refresh();
    },
  });
}

/** Adds an entry (with the form's Idempotency-Key) or changes one (with its version in If-Match). */
export const useSave = <Body>(
  writer: Pick<Writer<Body>, 'create' | 'update'>,
) =>
  useWrite(({ body, edit, idempotencyKey }: SaveInput<Body>) =>
    edit ? writer.update(edit, body) : writer.create(body, idempotencyKey),
  );

/**
 * Changes one known record that is never added or deleted from a form (the budget settings, recording a plan): the
 * record and its version are bound by the caller, so the form's `edit` and key are not used.
 */
export const useChange = <Body>(send: (body: Body) => Promise<unknown>) =>
  useWrite(({ body }: SaveInput<Body>) => send(body));

/** Deletes an entry by its id. */
export const useRemove = (remove: Writer<unknown>['remove']) =>
  useWrite(remove);

/**
 * The Idempotency-Key of one opened add form (docs/rules.md, "Writes"): the same key while the same data is retried,
 * a new one when the data changes. A form that is opened again mounts again and starts with a new key.
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
    /** Forgets the key, after the API said it was used for other data. */
    renew: () => {
      last.current = null;
    },
  };
}
