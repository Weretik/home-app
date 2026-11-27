import {
  onlineManager,
  QueryClient,
  QueryClientProvider,
} from '@tanstack/react-query';
import { act, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach } from 'vitest';
import {
  recordRefused,
  refusedWrites,
} from '../../platform/offline/network-status';
import { OfflineStatus } from './OfflineStatus';

function renderStatus() {
  const queryClient = new QueryClient();
  render(
    <QueryClientProvider client={queryClient}>
      <OfflineStatus />
    </QueryClientProvider>,
  );
  return queryClient;
}

/** A queued write that waits: paused, since the device is offline. */
function queueWrite(queryClient: QueryClient) {
  void queryClient
    .getMutationCache()
    .build(queryClient, {
      mutationKey: ['queued-write'],
      mutationFn: () => Promise.resolve(),
      networkMode: 'online',
    })
    .execute(undefined);
}

afterEach(() => {
  act(() => onlineManager.setOnline(true));
  act(() => refusedWrites.clear());
});

test('shows nothing online with nothing waiting', () => {
  renderStatus();

  expect(screen.queryByRole('status')).not.toBeInTheDocument();
});

test('says «Офлайн» without a network, with the count of waiting writes', async () => {
  const queryClient = renderStatus();

  act(() => onlineManager.setOnline(false));
  expect(screen.getByRole('status')).toHaveTextContent('Офлайн');

  await act(async () => queueWrite(queryClient));
  await act(async () => queueWrite(queryClient));
  expect(
    screen.getByRole('status', { name: 'Офлайн, очікує: 2' }),
  ).toHaveTextContent('Офлайн · 2');
});

test('says «Очікує: N» online while queued writes are still on their way', async () => {
  const queryClient = renderStatus();
  act(() => onlineManager.setOnline(false));
  await act(async () => queueWrite(queryClient));

  // Online again but not yet sent: the write is still pending.
  queryClient.getMutationCache().getAll()[0]!.options.mutationFn = () =>
    new Promise(() => {});
  act(() => onlineManager.setOnline(true));

  expect(screen.getByRole('status')).toHaveTextContent('Очікує: 1');
});

test('says which queued write the server refused, and closes', async () => {
  renderStatus();

  act(() => recordRefused('товари «молоко»'));

  expect(
    screen.getByText('Не вдалося зберегти: товари «молоко»'),
  ).toBeInTheDocument();
  await userEvent.click(screen.getByRole('button', { name: 'Закрити' }));
  expect(refusedWrites.current()).toEqual([]);
});
