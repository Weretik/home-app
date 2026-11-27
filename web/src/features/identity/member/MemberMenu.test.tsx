import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/query-client';
import { session } from '../../../platform/session';
import { MemberMenu } from './MemberMenu';

const member = {
  email: 'v@example.com',
  name: 'Віталій',
  pictureUrl: null,
  person: 'Vitaliy',
};

function renderMenu(signOutStatus = 204) {
  const requests: Request[] = [];
  vi.stubGlobal(
    'fetch',
    vi.fn((request: Request) => {
      requests.push(request);
      if (request.method === 'POST')
        return Promise.resolve(new Response(null, { status: signOutStatus }));
      return Promise.resolve(
        new Response(JSON.stringify(member), {
          headers: { 'Content-Type': 'application/json' },
        }),
      );
    }),
  );
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  client.setQueryData(['expenses', 'list'], ['cached']);
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter>
          <MemberMenu />
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  return { requests, client };
}

beforeEach(() => {
  localStorage.clear();
  session.save('token-1');
});
afterEach(() => vi.unstubAllGlobals());

test('shows the first letter, then the name and e-mail in the menu', async () => {
  renderMenu();

  const button = await screen.findByRole('button', {
    name: 'Акаунт: Віталій',
  });
  expect(button).toHaveTextContent('В');
  await userEvent.click(button);

  expect(screen.getByText('Віталій')).toBeInTheDocument();
  expect(screen.getByText('v@example.com')).toBeInTheDocument();
  expect(screen.getByRole('menuitem', { name: 'Профіль' })).toHaveAttribute(
    'href',
    '/profile',
  );
});

test('«Вийти» ends the session on the server, forgets the token and clears the cache', async () => {
  const { requests, client } = renderMenu();

  await userEvent.click(
    await screen.findByRole('button', { name: 'Акаунт: Віталій' }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Вийти' }));

  await waitFor(() =>
    expect(session.current()).toEqual({ token: null, ended: 'signed-out' }),
  );
  const signOut = requests.find((r) => r.method === 'POST');
  expect(signOut?.url).toMatch(/\/api\/identity\/sign-out$/);
  expect(signOut?.headers.get('Authorization')).toBe('Bearer token-1');
  expect(client.getQueryData(['expenses', 'list'])).toBeUndefined();
});

test('«Вийти» signs this device out even when the server answers 401', async () => {
  renderMenu(401);

  await userEvent.click(
    await screen.findByRole('button', { name: 'Акаунт: Віталій' }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Вийти' }));

  await waitFor(() =>
    expect(session.current()).toEqual({ token: null, ended: 'signed-out' }),
  );
});
