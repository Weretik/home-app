import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/query-client';
import { session } from '../../../platform/session';
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
) {
  const requests: Request[] = [];
  vi.stubGlobal(
    'fetch',
    vi.fn((request: Request) => {
      requests.push(request);
      if (request.method === 'POST')
        return Promise.resolve(new Response(null, { status: 204 }));
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

test('links to the notes about each other «Про нас»', async () => {
  renderProfile(masha);

  await screen.findByText('Марія Ціупіна');
  expect(screen.getByRole('link', { name: 'Відкрити' })).toHaveAttribute(
    'href',
    '/notes',
  );
});
