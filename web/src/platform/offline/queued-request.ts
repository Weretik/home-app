import type { moduleKeys } from '../client/query-client';

/**
 * A write that waits for the network (web ADR 0020): plain data, so it is kept in IndexedDB with the paused mutation
 * and sent after a restart without the screen that made it.
 */
export interface QueuedRequest {
  method: 'post' | 'put' | 'delete' | 'patch';
  /** The concrete path, ids filled in: `/api/todos/tasks/3f…/status`. */
  path: string;
  body?: unknown;
  /** Extra headers: an `Idempotency-Key` of a create, the `If-Match` version of an edit. */
  headers?: Record<string, string>;
  /** The backend module whose queries refresh after the write (ADR 0009). */
  module: keyof typeof moduleKeys;
  /** What the write does, in Ukrainian, for «Не вдалося зберегти: …». */
  label: string;
  /** The GET path that reads the record again for its `version` when the write is refused as stale. */
  refetch?: string;
}

/** What a queued write mutation is called with: the request, and the feature's own input for its callbacks. */
export interface QueuedVariables<Input = unknown> {
  request: QueuedRequest;
  input: Input;
}

/** Every queued write's mutation key starts with this; the persister keeps only these mutations. */
export const queuedWriteKey = ['queued-write'] as const;

export const isQueuedWriteKey = (key: readonly unknown[] | undefined) =>
  key?.[0] === queuedWriteKey[0];
