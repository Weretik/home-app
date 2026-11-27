import {
  onlineManager,
  QueryClient,
  QueryClientProvider,
} from '@tanstack/react-query';
import { act, renderHook, waitFor } from '@testing-library/react';
import type { ReactNode } from 'react';
import { afterEach, vi } from 'vitest';
import { refusedWrites } from '../status/network-status';
import { useQueuedWrite } from './use-queued-write';

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  });

const toRequest = (name: string) => ({
  method: 'post' as const,
  path: '/api/shopping/items',
  body: { text: name },
  module: 'shopping' as const,
  label: `товари «${name}»`,
});

function setup(answer: () => Response) {
  const fetch = vi.fn(() => Promise.resolve(answer()));
  vi.stubGlobal('fetch', fetch);
  const queryClient = new QueryClient();
  const invalidate = vi.spyOn(queryClient, 'invalidateQueries');
  const onMutate = vi.fn();
  const wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
  const { result } = renderHook(() => useQueuedWrite(toRequest, { onMutate }), {
    wrapper,
  });
  return { result, fetch, invalidate, onMutate, queryClient };
}

afterEach(() => {
  vi.unstubAllGlobals();
  onlineManager.setOnline(true);
  refusedWrites.clear();
});

test('runs the feature’s optimistic change with its own input and refreshes the module after', async () => {
  const { result, invalidate, onMutate, queryClient } = setup(() => json({}));

  act(() => result.current.mutate('молоко'));

  await waitFor(() => expect(result.current.isSuccess).toBe(true));
  expect(onMutate).toHaveBeenCalledWith('молоко');
  expect(invalidate).toHaveBeenCalledWith({ queryKey: ['shopping'] });
  const [mutation] = queryClient.getMutationCache().getAll();
  expect(mutation!.options.mutationKey).toEqual(['queued-write']);
  expect(mutation!.state.variables).toEqual({
    request: toRequest('молоко'),
    input: 'молоко',
  });
});

test('offline it pauses and is not «saving»; when the network is back it is sent', async () => {
  const { result, fetch } = setup(() => json({}));
  onlineManager.setOnline(false);

  act(() => result.current.mutate('молоко'));

  await waitFor(() => expect(result.current.isPaused).toBe(true));
  expect(result.current.isSaving).toBe(false);
  expect(fetch).not.toHaveBeenCalled();

  act(() => onlineManager.setOnline(true));
  await waitFor(() => expect(result.current.isSuccess).toBe(true));
  expect(fetch).toHaveBeenCalledTimes(1);
});

test('a refusal of a write made offline is recorded; one made online is left to the screen', async () => {
  const { result } = setup(() => json({ code: 'validation' }, 400));

  act(() => result.current.mutate('онлайн'));
  await waitFor(() => expect(result.current.isError).toBe(true));
  expect(refusedWrites.current()).toEqual([]);

  onlineManager.setOnline(false);
  act(() => result.current.mutate('офлайн'));
  act(() => onlineManager.setOnline(true));
  await waitFor(() => expect(refusedWrites.current()).toHaveLength(1));
  expect(refusedWrites.current()[0]!.label).toBe('товари «офлайн»');
});
