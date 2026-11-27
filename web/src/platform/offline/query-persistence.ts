import { createAsyncStoragePersister } from '@tanstack/query-async-storage-persister';
import type { Mutation, Query } from '@tanstack/react-query';
import type {
  PersistQueryClientOptions,
  Persister,
} from '@tanstack/react-query-persist-client';
import { createStore, del, get, set } from 'idb-keyval';
import { QUERY_GC_TIME } from '../client/query-client';
import { isQueuedWriteKey } from './queued-request';

/** How long the data last seen is kept for offline use (web ADR 0020); queries' `gcTime` is the same. */
export const CACHE_MAX_AGE = QUERY_GC_TIME;

/** The IndexedDB database the cache lives in. */
export const CACHE_DATABASE = 'home-cache';

/** The key-value storage the persister writes to: IndexedDB in the browser, a map in tests. */
export interface CacheStorage {
  getItem: (key: string) => Promise<string | null | undefined>;
  setItem: (key: string, value: string) => Promise<void>;
  removeItem: (key: string) => Promise<void>;
}

/** IndexedDB storage, or undefined where there is none (jsdom, some private windows): then nothing is kept. */
export function indexedDbStorage(): CacheStorage | undefined {
  if (typeof indexedDB === 'undefined') return undefined;
  const store = createStore(CACHE_DATABASE, 'queries');
  return {
    getItem: (key) => get<string>(key, store),
    setItem: (key, value) => set(key, value, store),
    removeItem: (key) => del(key, store),
  };
}

/** Only data that loaded: a failed or pending query is not worth showing offline. */
export const shouldKeepQuery = (query: Query) =>
  query.state.status === 'success';

/** Only queued writes that wait for the network; other mutations are never replayed. */
export const shouldKeepMutation = (mutation: Mutation) =>
  mutation.state.isPaused && isQueuedWriteKey(mutation.options.mutationKey);

export type CachePersistence = Omit<
  PersistQueryClientOptions,
  'queryClient'
> & {
  persister: Persister;
};

/**
 * What `PersistQueryClientProvider` needs, or undefined without storage. `buster` is the API contract's hash: a new
 * contract drops the kept data, whose shape may have changed.
 */
export function cachePersistence(
  storage: CacheStorage | undefined,
  buster: string,
): CachePersistence | undefined {
  if (!storage) return undefined;
  return {
    persister: createAsyncStoragePersister({
      storage,
      key: 'react-query',
      throttleTime: 1000,
    }),
    maxAge: CACHE_MAX_AGE,
    buster,
    dehydrateOptions: {
      shouldDehydrateQuery: shouldKeepQuery,
      shouldDehydrateMutation: shouldKeepMutation,
    },
  };
}
