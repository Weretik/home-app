import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/query-client';
import type { NotificationsStatus } from '../data/notifications-api';
import { NotificationsPage } from './NotificationsPage';

afterEach(() => vi.unstubAllGlobals());

const status = (
  overrides: Partial<NotificationsStatus> = {},
): NotificationsStatus => ({
  configured: true,
  botUsername: 'home_bot',
  botLink: 'https://t.me/home_bot',
  chats: [
    {
      audience: 'Vitaliy',
      linked: true,
      title: 'Виталий',
      linkedAtUtc: '2026-11-27T06:00:00Z',
    },
    { audience: 'Maria', linked: false, title: null, linkedAtUtc: null },
  ],
  schedule: [
    {
      kind: 'TodosMorning',
      title: 'Утренние дела',
      when: 'каждый день в 8:00',
    },
  ],
  deliveries: [
    {
      kind: 'TodosMorning',
      audience: 'Vitaliy',
      key: 'TodosMorning:2026-11-27:Vitaliy',
      createdAtUtc: '2026-11-27T06:00:00Z',
      sentAtUtc: null,
    },
  ],
  ...overrides,
});

const json = (body: unknown, code = 200) =>
  Promise.resolve(
    new Response(body === undefined ? null : JSON.stringify(body), {
      status: code,
      headers: { 'Content-Type': 'application/json' },
    }),
  );

function renderPage(body: NotificationsStatus) {
  const writes: Request[] = [];
  vi.stubGlobal(
    'fetch',
    vi.fn((input: Request) => {
      if (input.method === 'GET') return json(body);
      writes.push(input);
      return json(undefined, 204);
    }),
  );
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <NotificationsPage />
      </QueryClientProvider>
    </ThemeProvider>,
  );
  return writes;
}

test('shows the bot, the chats, the schedule and the deliveries', async () => {
  renderPage(status());

  expect(await screen.findByText('@home_bot')).toBeInTheDocument();
  expect(screen.getByRole('link', { name: 'Открыть бота' })).toHaveAttribute(
    'href',
    'https://t.me/home_bot',
  );
  const chats = screen.getByRole('region', { name: 'Чаты' });
  expect(within(chats).getByText('Семейная группа')).toBeInTheDocument();
  expect(within(chats).getAllByText('не подключён')).toHaveLength(2);
  expect(within(chats).getByText('Виталий · с 27.11')).toBeInTheDocument();
  const schedule = screen.getByRole('region', { name: 'Расписание' });
  expect(within(schedule).getByText('каждый день в 8:00')).toBeInTheDocument();
  const deliveries = screen.getByRole('region', { name: 'Последние отправки' });
  expect(within(deliveries).getByText('Утренние дела')).toBeInTheDocument();
  expect(within(deliveries).getByText('в очереди')).toBeInTheDocument();
});

test('says what to set when the bot has no token', async () => {
  renderPage(
    status({
      configured: false,
      botUsername: null,
      botLink: null,
      chats: [],
      deliveries: [],
    }),
  );

  expect(await screen.findByText(/Бот ещё не настроен/)).toBeInTheDocument();
  expect(
    screen.queryByRole('region', { name: 'Последние отправки' }),
  ).toBeNull();
});

test('unlinks a chat after asking', async () => {
  const user = userEvent.setup();
  const writes = renderPage(status());

  await user.click(
    await screen.findByRole('button', { name: 'Отвязать: Виталий' }),
  );
  const dialog = screen.getByRole('dialog', {
    name: 'Отвязать чат «Виталий»?',
  });
  await user.click(within(dialog).getByRole('button', { name: 'Отвязать' }));

  await waitFor(() => expect(writes).toHaveLength(1));
  expect(writes[0]?.method).toBe('DELETE');
  expect(new URL(writes[0]!.url).pathname).toBe(
    '/api/notifications/chats/Vitaliy',
  );
  expect(await screen.findByText('Чат «Виталий» отвязан')).toBeInTheDocument();
});

test('sends a test message at once', async () => {
  const user = userEvent.setup();
  const writes = renderPage(status());

  await user.click(
    await screen.findByRole('button', { name: 'Тест: Виталий' }),
  );

  await waitFor(() => expect(writes).toHaveLength(1));
  expect(writes[0]?.method).toBe('POST');
  expect(await writes[0]!.json()).toEqual({ audience: 'Vitaliy' });
  expect(
    await screen.findByText('Тест отправлен: проверьте Телеграм'),
  ).toBeInTheDocument();
});
