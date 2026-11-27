import { onlineManager, QueryClient } from '@tanstack/react-query';
import { afterEach, vi } from 'vitest';
import {
  countQueuedWrites,
  isOnline,
  recordRefused,
  refusedWrites,
  subscribeOnline,
} from './network-status';

afterEach(() => {
  onlineManager.setOnline(true);
  refusedWrites.clear();
});

test('follows the online state of React Query', () => {
  const listener = vi.fn();
  const unsubscribe = subscribeOnline(listener);

  onlineManager.setOnline(false);

  expect(isOnline()).toBe(false);
  expect(listener).toHaveBeenCalled();
  unsubscribe();
});

test('counts only queued writes that wait', async () => {
  onlineManager.setOnline(false);
  const queryClient = new QueryClient();
  const cache = queryClient.getMutationCache();
  const build = (key: string[]) =>
    cache.build(queryClient, {
      mutationKey: key,
      mutationFn: () => Promise.resolve(),
      networkMode: 'online',
    });

  void build(['queued-write', 'shopping']).execute(undefined);
  void build(['queued-write']).execute(undefined);
  void build(['other']).execute(undefined);
  await Promise.resolve();

  expect(countQueuedWrites(cache)).toBe(2);
});

test('keeps the refused writes in order until each is dismissed', () => {
  const listener = vi.fn();
  const unsubscribe = refusedWrites.subscribe(listener);

  recordRefused('товари «молоко»');
  recordRefused('товари «молоко»');
  const [first, second] = refusedWrites.current();

  expect(first!.id).not.toBe(second!.id);
  refusedWrites.dismiss(first!.id);
  expect(refusedWrites.current()).toEqual([second]);
  expect(listener).toHaveBeenCalledTimes(3);
  unsubscribe();
});

test('a snapshot stays the same until something changes', () => {
  recordRefused('прибирання кошика');

  expect(refusedWrites.current()).toBe(refusedWrites.current());
});
