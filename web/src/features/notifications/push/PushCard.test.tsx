import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/client/query-client';
import type { PushStatus } from '../data/push-api';
import { PushCard } from './PushCard';

const endpoint = 'https://push.example/this-phone';
const status = (over: Partial<PushStatus> = {}): PushStatus => ({
  configured: true,
  publicKey: 'AP8-_w',
  canSubscribe: true,
  devices: [
    {
      id: 'd1',
      device: 'Android · Chrome',
      endpoint,
      createdAtUtc: '2026-11-27T06:00:00Z',
      lastSeenAtUtc: '2026-11-27T06:00:00Z',
    },
    {
      id: 'd2',
      device: 'Windows · Edge',
      endpoint: 'https://push.example/laptop',
      createdAtUtc: '2026-11-26T06:00:00Z',
      lastSeenAtUtc: '2026-11-26T06:00:00Z',
    },
  ],
  ...over,
});

const json = (body: unknown, code = 200) =>
  Promise.resolve(
    new Response(body === undefined ? null : JSON.stringify(body), {
      status: code,
      headers: { 'Content-Type': 'application/json' },
    }),
  );

afterEach(() => vi.unstubAllGlobals());

/** A browser with push whose service worker already holds the subscription of `endpoint`. */
function stubBrowser(subscribed: string | null) {
  const unsubscribe = vi.fn(() => Promise.resolve(true));
  const subscription = subscribed && {
    endpoint: subscribed,
    unsubscribe,
    toJSON: () => ({ endpoint: subscribed, keys: { p256dh: 'p', auth: 'a' } }),
  };
  vi.stubGlobal('PushManager', function PushManager() {});
  vi.stubGlobal('Notification', {
    permission: 'granted',
    requestPermission: () => Promise.resolve('granted'),
  });
  Object.defineProperty(navigator, 'serviceWorker', {
    configurable: true,
    value: {
      ready: Promise.resolve({
        pushManager: {
          getSubscription: () => Promise.resolve(subscription),
          subscribe: () => Promise.resolve(subscription),
        },
      }),
    },
  });
  return unsubscribe;
}

function renderCard(body: PushStatus) {
  const writes: Request[] = [];
  vi.stubGlobal(
    'fetch',
    vi.fn((input: Request) => {
      if (input.method === 'GET') return json(body);
      writes.push(input);
      if (input.url.endsWith('/push/test')) return json({ delivered: 2 });
      return json(undefined, 204);
    }),
  );
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <PushCard />
      </QueryClientProvider>
    </ThemeProvider>,
  );
  return writes;
}

test('says push is not set up without keys', async () => {
  renderCard(status({ configured: false, publicKey: null, devices: [] }));

  expect(
    await screen.findByText(/Push-сповіщення ще не налаштовано/),
  ).toBeInTheDocument();
  expect(screen.queryByRole('switch')).not.toBeInTheDocument();
});

test('marks this device, sends a test and removes another device', async () => {
  stubBrowser(endpoint);
  const writes = renderCard(status());

  expect(await screen.findByText('цей пристрій')).toBeInTheDocument();
  expect(await screen.findByRole('switch')).toBeChecked();

  await userEvent.click(screen.getByRole('button', { name: 'Надіслати тест' }));
  expect(
    await screen.findByText('Надіслано на пристрої: 2'),
  ).toBeInTheDocument();

  await userEvent.click(screen.getByRole('button', { name: 'Видалити' }));
  expect(writes.map((w) => `${w.method} ${new URL(w.url).pathname}`)).toEqual([
    'POST /api/notifications/push/test',
    'DELETE /api/notifications/push/subscriptions/d2',
  ]);
});

test('turning the switch off unsubscribes the browser and removes this device', async () => {
  const unsubscribe = stubBrowser(endpoint);
  const writes = renderCard(status());

  const toggle = await screen.findByRole('switch');
  await screen.findByText('цей пристрій');
  await userEvent.click(toggle);

  await vi.waitFor(() => expect(unsubscribe).toHaveBeenCalled());
  await vi.waitFor(() =>
    expect(writes.map((w) => `${w.method} ${new URL(w.url).pathname}`)).toEqual(
      ['DELETE /api/notifications/push/subscriptions/d1'],
    ),
  );
});

test('turning the switch on subscribes and saves the subscription', async () => {
  stubBrowser('https://push.example/new');
  const writes = renderCard(status({ devices: [] }));

  await userEvent.click(await screen.findByRole('switch'));

  await vi.waitFor(() => expect(writes).toHaveLength(1));
  expect(writes[0]!.method).toBe('PUT');
  expect(await writes[0]!.json()).toMatchObject({
    endpoint: 'https://push.example/new',
    p256dh: 'p',
    auth: 'a',
  });
});
