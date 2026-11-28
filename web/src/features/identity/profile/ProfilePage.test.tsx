import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/client/query-client';
import { session } from '../../../platform/session/session';
import { ProfilePage } from './ProfilePage';

const masha = {
  email: 'masha@family.test',
  name: 'Марія Ціупіна',
  pictureUrl: null,
  person: 'Maria',
};

const chat = (audience: string, linked: boolean) => ({
  audience,
  linked,
  title: linked ? `@${audience.toLowerCase()}` : null,
  linkedAtUtc: linked ? '2026-11-27T00:00:00Z' : null,
});

function renderProfile(
  member: object,
  chats = [chat('Vitaliy', false), chat('Maria', true)],
  postStatus = 204,
) {
  const requests: Request[] = [];
  vi.stubGlobal(
    'fetch',
    vi.fn((request: Request) => {
      requests.push(request);
      if (request.method === 'POST')
        return Promise.resolve(
          postStatus === 204
            ? new Response(null, { status: 204 })
            : new Response(JSON.stringify({ title: 'Помилка сервера' }), {
                status: postStatus,
                headers: { 'Content-Type': 'application/problem+json' },
              }),
        );
      const body = request.url.endsWith('/api/identity/me')
        ? member
        : {
            configured: true,
            botUsername: 'bot',
            botLink: null,
            chats,
            schedule: [],
            deliveries: [],
          };
      return Promise.resolve(
        new Response(JSON.stringify(body), {
          headers: { 'Content-Type': 'application/json' },
        }),
      );
    }),
  );
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter>
          <ProfilePage />
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  return requests;
}

beforeEach(() => {
  localStorage.clear();
  session.save('token-1');
});
afterEach(() => vi.unstubAllGlobals());

test('shows the name, e-mail, who the member is and their Telegram chat', async () => {
  renderProfile(masha);

  expect(await screen.findByText('Марія Ціупіна')).toBeInTheDocument();
  expect(screen.getByText('masha@family.test')).toBeInTheDocument();
  expect(screen.getByText('Маша')).toBeInTheDocument();
  expect(await screen.findByText('Підключено: @maria')).toBeInTheDocument();
  expect(screen.getByRole('link', { name: 'Налаштування' })).toHaveAttribute(
    'href',
    '/notifications',
  );
});

test('offers to link Telegram when the chat is not linked', async () => {
  renderProfile({ ...masha, person: 'Vitaliy' });

  expect(await screen.findByText('Не підключено')).toBeInTheDocument();
  expect(screen.getByText('Віталій')).toBeInTheDocument();
  expect(screen.getByRole('link', { name: 'Підключити' })).toBeInTheDocument();
});

test('a member who is neither of the two is a guest and is not asked about Telegram', async () => {
  const requests = renderProfile({ ...masha, person: null });

  expect(await screen.findByText('Гість')).toBeInTheDocument();
  expect(screen.getByText('Бот пише лише Віталію та Маші')).toBeInTheDocument();
  expect(requests.some((r) => r.url.includes('/notifications/'))).toBe(false);
});

test('«Вийти» ends the session', async () => {
  renderProfile(masha);

  await screen.findByText('Марія Ціупіна');
  await userEvent.click(screen.getByRole('button', { name: 'Вийти' }));

  await waitFor(() =>
    expect(session.current()).toEqual({ token: null, ended: 'signed-out' }),
  );
});

test('«Вийти на всіх пристроях» asks first, then ends every session and this one', async () => {
  const requests = renderProfile(masha);

  await screen.findByText('Марія Ціупіна');
  await userEvent.click(
    screen.getByRole('button', { name: 'Вийти на всіх пристроях' }),
  );
  const dialog = await screen.findByRole('dialog', {
    name: 'Вийти на всіх пристроях?',
  });
  expect(requests.some((r) => r.method === 'POST')).toBe(false);

  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Вийти всюди' }),
  );

  await waitFor(() =>
    expect(session.current()).toEqual({ token: null, ended: 'signed-out' }),
  );
  const post = requests.find((r) => r.method === 'POST');
  expect(new URL(post!.url).pathname).toBe('/api/identity/sign-out-everywhere');
});

test('«Вийти на всіх пристроях» keeps this device signed in when the server did not confirm', async () => {
  renderProfile(masha, undefined, 500);

  await screen.findByText('Марія Ціупіна');
  await userEvent.click(
    screen.getByRole('button', { name: 'Вийти на всіх пристроях' }),
  );
  const dialog = await screen.findByRole('dialog');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Вийти всюди' }),
  );

  expect(await within(dialog).findByRole('alert')).toHaveTextContent(
    'Не вдалося вийти на всіх пристроях',
  );
  expect(session.current().token).toBe('token-1');
});

test('links to the notes about each other «Про нас»', async () => {
  renderProfile(masha);

  await screen.findByText('Марія Ціупіна');
  expect(screen.getByRole('link', { name: 'Відкрити' })).toHaveAttribute(
    'href',
    '/notes',
  );
});
