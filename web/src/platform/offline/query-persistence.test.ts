import {
  MutationCache,
  QueryClient,
  dehydrate,
  type Mutation,
  type Query,
} from '@tanstack/react-query';
import {
  persistQueryClientRestore,
  persistQueryClientSave,
} from '@tanstack/react-query-persist-client';
import {
  CACHE_MAX_AGE,
  cachePersistence,
  indexedDbStorage,
  shouldKeepMutation,
  shouldKeepQuery,
  type CacheStorage,
} from './query-persistence';

/** An in-memory storage in place of IndexedDB, which jsdom does not have. */
function memoryStorage() {
  const items = new Map<string, string>();
  const storage: CacheStorage = {
    getItem: (key) => Promise.resolve(items.get(key) ?? null),
    setItem: async (key, value) => void items.set(key, value),
    removeItem: async (key) => void items.delete(key),
  };
  return { storage, items };
}

test('keeps nothing where there is no IndexedDB', () => {
  expect(indexedDbStorage()).toBeUndefined();
  expect(cachePersistence(undefined, 'v1')).toBeUndefined();
});

test('keeps data for 7 days, versioned by the API contract', () => {
  const persistence = cachePersistence(memoryStorage().storage, 'hash-1');

  expect(persistence?.maxAge).toBe(CACHE_MAX_AGE);
  expect(CACHE_MAX_AGE).toBe(7 * 24 * 60 * 60 * 1000);
  expect(persistence?.buster).toBe('hash-1');
});

test('keeps every query with data, also after a failed refresh, and paused queued writes', () => {
  const query = (status: string, data?: unknown) =>
    ({ state: { status, data } }) as unknown as Query;
  const mutation = (key: string[], isPaused: boolean) =>
    ({
      options: { mutationKey: key },
      state: { isPaused },
    }) as unknown as Mutation;

  expect(shouldKeepQuery(query('success', { items: [] }))).toBe(true);
  // A polled list whose last refresh failed offline still has its data.
  expect(shouldKeepQuery(query('error', { items: [] }))).toBe(true);
  expect(shouldKeepQuery(query('error'))).toBe(false);
  expect(shouldKeepQuery(query('pending'))).toBe(false);
  expect(shouldKeepMutation(mutation(['queued-write', 'shopping'], true))).toBe(
    true,
  );
  expect(shouldKeepMutation(mutation(['queued-write'], false))).toBe(false);
  expect(shouldKeepMutation(mutation(['expenses'], true))).toBe(false);
});

test('restores the saved data, and drops it for another contract version', async () => {
  const { storage, items } = memoryStorage();
  const saved = new QueryClient({ mutationCache: new MutationCache() });
  saved.setQueryData(['shopping', 'list'], { items: ['молоко'] });
  const options = cachePersistence(storage, 'v1')!;
  await persistQueryClientSave({ ...options, queryClient: saved });
  expect(items.size).toBe(1);

  const restored = new QueryClient();
  await persistQueryClientRestore({ ...options, queryClient: restored });
  expect(restored.getQueryData(['shopping', 'list'])).toEqual({
    items: ['молоко'],
  });

  const newer = new QueryClient();
  await persistQueryClientRestore({
    ...cachePersistence(storage, 'v2')!,
    queryClient: newer,
  });
  expect(newer.getQueryData(['shopping', 'list'])).toBeUndefined();
  expect(dehydrate(newer).queries).toHaveLength(0);
});
