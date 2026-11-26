import {
  useMutation,
  useQueryClient,
  type QueryKey,
} from '@tanstack/react-query';
import { useRef } from 'react';
import { newIdempotencyKey } from '../../../api-client';
import { isStale } from './write-errors-api';

/** The item an edit starts from: its id and, where the API checks it, the version the page showed (If-Match). */
export interface EditedEntry {
  id: string;
  version?: number;
}

export interface SaveInput<Body> {
  body: Body;
  /** Set for an edit; a create leaves it out. */
  edit?: EditedEntry;
  /** Sent with a create (see {@link useIdempotencyKey}). */
  idempotencyKey: string;
}

/** How one kind of item is written, and which queries show it. */
export interface Writer<Body> {
  keys: QueryKey[];
  create: (body: Body, idempotencyKey: string) => Promise<unknown>;
  update: (entry: EditedEntry, body: Body) => Promise<unknown>;
  remove: (id: string) => Promise<unknown>;
}

/** The path id and the If-Match version of an edit. */
export const ifMatch = (entry: EditedEntry) => ({
  path: { id: entry.id },
  header: { 'If-Match': String(entry.version ?? 0) },
});

/** Refreshes the given queries; used after a success and after a stale failure. */
export function useRefresh(keys: QueryKey[]) {
  const queryClient = useQueryClient();
  return () => {
    for (const queryKey of keys)
      void queryClient.invalidateQueries({ queryKey });
  };
}

/** Adds or changes an item of `writer`'s kind; refreshes its queries, also when the item turned out stale. */
export function useSave<Body>(writer: Writer<Body>) {
  const refresh = useRefresh(writer.keys);
  return useMutation({
    mutationFn: ({ body, edit, idempotencyKey }: SaveInput<Body>) =>
      edit ? writer.update(edit, body) : writer.create(body, idempotencyKey),
    onSuccess: refresh,
    onError: (error) => {
      if (isStale(error)) refresh();
    },
  });
}

/** Deletes an item of `writer`'s kind; refreshes its queries. */
export function useRemove<Body>(writer: Writer<Body>) {
  const refresh = useRefresh(writer.keys);
  return useMutation({
    mutationFn: (id: string) => writer.remove(id),
    onSuccess: refresh,
    onError: (error) => {
      if (isStale(error)) refresh();
    },
  });
}

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
