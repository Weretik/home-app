import { QueryClientProvider } from '@tanstack/react-query';
import { act, renderHook, waitFor } from '@testing-library/react';
import type { ReactNode } from 'react';
import { ApiError } from '../../../api-client';
import { createQueryClient } from '../../../platform/client/query-client';
import { useChange, useRemove, useSave } from './write-hooks-api';

// A query of every kind the Expenses module shows, the home summary among them (features never import each other,
// so the summary's key is written out here as the contract: it starts with the module's prefix).
const shownKeys = [
  ['expenses', 'list', { year: 2026, month: 10 }, {}],
  ['expenses', 'budget'],
  ['expenses', 'planned', { year: 2026, month: 10 }],
  ['expenses', 'summary', { year: 2026, month: 10 }],
];

function setup() {
  const client = createQueryClient();
  for (const key of shownKeys) client.setQueryData(key, {});
  const wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={client}>{children}</QueryClientProvider>
  );
  const allStale = () =>
    shownKeys.every((key) => client.getQueryState(key)?.isInvalidated);
  return { wrapper, allStale };
}

const ok = () => Promise.resolve({});

test('a save refreshes every query of the module, the home summary too', async () => {
  const { wrapper, allStale } = setup();
  const { result } = renderHook(
    () => useSave<string>({ create: ok, update: ok }),
    { wrapper },
  );

  act(() => result.current.mutate({ body: 'x', idempotencyKey: 'k' }));

  await waitFor(() => expect(allStale()).toBe(true));
});

test('a delete refreshes the module too', async () => {
  const { wrapper, allStale } = setup();
  const { result } = renderHook(() => useRemove(ok), { wrapper });

  act(() => result.current.mutate('id'));

  await waitFor(() => expect(allStale()).toBe(true));
});

test('a change of a known record refreshes the module too', async () => {
  const { wrapper, allStale } = setup();
  const { result } = renderHook(() => useChange<string>(ok), { wrapper });

  act(() => result.current.mutate({ body: 'x', idempotencyKey: 'k' }));

  await waitFor(() => expect(allStale()).toBe(true));
});

test('a stale entry refreshes the module; another failure does not', async () => {
  const stale = setup();
  const failing = (status: number, code?: string) => () =>
    Promise.reject(new ApiError(status, { title: 'fail', code }));
  const { result } = renderHook(
    () => useSave<string>({ create: ok, update: failing(404) }),
    { wrapper: stale.wrapper },
  );
  act(() =>
    result.current.mutate({
      body: 'x',
      edit: { id: '1', version: 1 },
      idempotencyKey: 'k',
    }),
  );
  await waitFor(() => expect(stale.allStale()).toBe(true));

  const other = setup();
  const second = renderHook(
    () => useSave<string>({ create: failing(500), update: ok }),
    { wrapper: other.wrapper },
  );
  act(() => second.result.current.mutate({ body: 'x', idempotencyKey: 'k' }));
  await waitFor(() => expect(second.result.current.isError).toBe(true));
  expect(other.allStale()).toBe(false);
});
