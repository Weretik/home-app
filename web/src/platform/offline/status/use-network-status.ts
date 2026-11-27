import { useQueryClient } from '@tanstack/react-query';
import { useCallback, useSyncExternalStore } from 'react';
import {
  countQueuedWrites,
  isOnline,
  refusedWrites,
  subscribeOnline,
} from './network-status';

/** True while the device has a network. */
export function useOnline(): boolean {
  return useSyncExternalStore(subscribeOnline, isOnline, () => true);
}

/** How many queued writes wait («Очікує: N»). */
export function useQueuedWriteCount(): number {
  const cache = useQueryClient().getMutationCache();
  const subscribe = useCallback(
    (listener: () => void) => cache.subscribe(listener),
    [cache],
  );
  return useSyncExternalStore(
    subscribe,
    () => countQueuedWrites(cache),
    () => 0,
  );
}

/** The refused queued writes, for the snackbar. */
export function useRefusedWrites() {
  return useSyncExternalStore(
    refusedWrites.subscribe,
    refusedWrites.current,
    refusedWrites.current,
  );
}
