import {
  onlineManager,
  type Mutation,
  type MutationCache,
} from '@tanstack/react-query';
import { isQueuedWriteKey } from './queued-request';

/** Whether the device has a network now (React Query's online manager, the same one that pauses queued writes). */
export const isOnline = () => onlineManager.isOnline();

/** Follows the online state: for `useSyncExternalStore`. */
export const subscribeOnline = (listener: () => void) =>
  onlineManager.subscribe(listener);

const isWaitingQueuedWrite = (mutation: Mutation<unknown, Error, unknown>) =>
  isQueuedWriteKey(mutation.options.mutationKey) &&
  mutation.state.status === 'pending';

/** How many queued writes wait to be sent or are on their way (paused offline or retrying). */
export const countQueuedWrites = (cache: MutationCache) =>
  cache.getAll().filter(isWaitingQueuedWrite).length;

/** A queued write the server refused; `id` keeps two refusals of the same label apart. */
export interface RefusedWrite {
  id: number;
  label: string;
}

let refused: readonly RefusedWrite[] = [];
let nextId = 1;
const listeners = new Set<() => void>();
const notify = () => listeners.forEach((listener) => listener());

/** The refused queued writes not yet dismissed, oldest first (one immutable snapshot per change). */
export const refusedWrites = {
  current: () => refused,
  subscribe: (listener: () => void) => {
    listeners.add(listener);
    return () => void listeners.delete(listener);
  },
  dismiss: (id: number) => {
    refused = refused.filter((write) => write.id !== id);
    notify();
  },
  clear: () => {
    refused = [];
    notify();
  },
};

/** Records a refused queued write for the snackbar «Не вдалося зберегти: <label>». */
export function recordRefused(label: string) {
  refused = [...refused, { id: nextId++, label }];
  notify();
}
