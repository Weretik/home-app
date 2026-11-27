import { QueryClientProvider } from '@tanstack/react-query';
import { renderHook, waitFor } from '@testing-library/react';
import type { ReactNode } from 'react';
import { afterEach, vi } from 'vitest';
import { createQueryClient } from '../../../../platform/query-client';
import { moduleKeys } from '../../../../platform/query-client';
import { useMonthSummary } from './summary-api';

afterEach(() => vi.unstubAllGlobals());

test('the summary is under the Expenses prefix, so every write to the module refreshes it', async () => {
  vi.stubGlobal('fetch', () =>
    Promise.resolve(
      new Response('{}', { headers: { 'Content-Type': 'application/json' } }),
    ),
  );
  const client = createQueryClient();
  const wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={client}>{children}</QueryClientProvider>
  );
  const { result } = renderHook(
    () => useMonthSummary({ year: 2026, month: 10 }),
    { wrapper },
  );
  await waitFor(() => expect(result.current.isSuccess).toBe(true));

  await client.invalidateQueries({ queryKey: moduleKeys.expenses });

  const [summary] = client.getQueryCache().getAll();
  expect(summary!.queryKey.slice(0, 1)).toEqual([...moduleKeys.expenses]);
  expect(client.getQueryState(summary!.queryKey)?.dataUpdateCount).toBe(2);
});
