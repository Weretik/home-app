import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { act, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, beforeEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/query-client';
import { session } from '../../../platform/session';
import type { GoogleAccountsId } from '../data/google-identity';
import { MemberMenu } from '../member/MemberMenu';
import { SessionGate } from './SessionGate';

const member = { email: 'v@example.com', name: 'Віталій', pictureUrl: null };

const json = (body: unknown, status = 200) =>
  Promise.resolve(
    new Response(JSON.stringify(body), {
      status,
      headers: { 'Content-Type': 'application/json' },
    }),
  );

/** A fake Google script: its button hands Google's credential to the app's callback. */
function fakeGoogle() {
  let callback: (response: { credential: string }) => void = () => {};
  const id: GoogleAccountsId = {
    initialize: vi.fn((config) => {
      callback = config.callback;
    }),
    renderButton: vi.fn((host: HTMLElement) => {
      const button = document.createElement('button');
      button.textContent = 'Sign in with Google';
      button.onclick = () => callback({ credential: 'google-token' });
      host.append(button);
    }),
    prompt: vi.fn(),
    disableAutoSelect: vi.fn(),
  };
  window.google = { accounts: { id } };
  return id;
}

function renderGate(answer: (request: Request) => Promise<Response>) {
  const requests: Request[] = [];
  vi.stubGlobal(
    'fetch',
    vi.fn((request: Request) => {
      requests.push(request);
      return answer(request);
    }),
  );
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <SessionGate>
          <p>Застосунок</p>
          <MemberMenu />
        </SessionGate>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  return requests;
}

beforeEach(() => {
  localStorage.clear();
  session.clear('signed-out');
  vi.stubEnv('VITE_GOOGLE_CLIENT_ID', 'client-1');
});

afterEach(() => {
  vi.unstubAllGlobals();
  vi.unstubAllEnvs();
  delete window.google;
});

test('without a token shows the sign-in screen instead of the app', async () => {
  fakeGoogle();
  renderGate(() => json(member));

  expect(
    screen.getByRole('heading', { name: 'Вхід до сімейного сайту' }),
  ).toBeInTheDocument();
  expect(screen.getByText('Вхід лише для родини')).toBeInTheDocument();
  expect(
    await screen.findByRole('button', { name: 'Sign in with Google' }),
  ).toBeInTheDocument();
  expect(screen.queryByText('Застосунок')).not.toBeInTheDocument();
});

test('with a stored token shows the app', () => {
  session.save('token-1');
  renderGate(() => json(member));

  expect(screen.getByText('Застосунок')).toBeInTheDocument();
});

test('signs in with the Google credential and shows the app', async () => {
  fakeGoogle();
  const requests = renderGate((request) =>
    request.url.endsWith('/api/identity/sign-in')
      ? json({ token: 'token-1', expiresAtUtc: '2026-12-27T00:00:00Z', member })
      : json(member),
  );

  await userEvent.click(
    await screen.findByRole('button', { name: 'Sign in with Google' }),
  );

  expect(await screen.findByText('Застосунок')).toBeInTheDocument();
  expect(localStorage.getItem('home.session')).toBe('token-1');
  const signIn = requests.find((r) => r.url.endsWith('/api/identity/sign-in'));
  expect(signIn?.method).toBe('POST');
  expect(await signIn?.json()).toEqual({ credential: 'google-token' });
});

test('says when the e-mail is not on the family list', async () => {
  fakeGoogle();
  renderGate(() =>
    json({ status: 403, code: 'identity.not_a_member', detail: 'No.' }, 403),
  );

  await userEvent.click(
    await screen.findByRole('button', { name: 'Sign in with Google' }),
  );

  expect(await screen.findByRole('alert')).toHaveTextContent(
    'Ця пошта не має доступу. Увійдіть акаунтом Віталія або Маші.',
  );
  expect(screen.queryByText('Застосунок')).not.toBeInTheDocument();
});

test('without a client id says sign-in is not set up', () => {
  vi.stubEnv('VITE_GOOGLE_CLIENT_ID', '');
  renderGate(() => json(member));

  expect(screen.getByText('Вхід ще не налаштовано')).toBeInTheDocument();
  expect(window.google).toBeUndefined();
});

test('a 401 ends the session and says it expired', async () => {
  session.save('token-1');
  renderGate(() => json({ status: 401 }, 401));

  expect(
    await screen.findByText('Сесія закінчилась, увійдіть знову'),
  ).toBeInTheDocument();
  expect(localStorage.getItem('home.session')).toBeNull();
});

test('follows a sign-out in another tab', () => {
  session.save('token-1');
  renderGate(() => json(member));

  act(() => {
    localStorage.removeItem('home.session');
    window.dispatchEvent(new StorageEvent('storage', { key: 'home.session' }));
  });

  expect(
    screen.getByRole('heading', { name: 'Вхід до сімейного сайту' }),
  ).toBeInTheDocument();
  expect(
    screen.queryByText('Сесія закінчилась, увійдіть знову'),
  ).not.toBeInTheDocument();
});

test('after «Вийти» Google does not sign the member straight back in', async () => {
  const id = fakeGoogle();
  session.save('token-1');
  renderGate(() => json(member));

  act(() => session.clear('signed-out'));

  expect(
    await screen.findByRole('button', { name: 'Sign in with Google' }),
  ).toBeInTheDocument();
  expect(id.disableAutoSelect).toHaveBeenCalled();
  expect(id.prompt).not.toHaveBeenCalled();
});

test('after an expired session Google may sign the member back in at once', async () => {
  const id = fakeGoogle();
  session.clear('expired');
  renderGate(() => json(member));

  await screen.findByRole('button', { name: 'Sign in with Google' });
  expect(id.prompt).toHaveBeenCalled();
  expect(id.disableAutoSelect).not.toHaveBeenCalled();
});
