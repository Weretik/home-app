import { afterEach, beforeEach, vi } from 'vitest';
import { createApiClient } from '../../api-client';
import { session } from './session';
import { sessionMiddleware } from './session-middleware';

const api = createApiClient({ baseUrl: 'http://api.test' });
api.use(sessionMiddleware);

const member = { email: 'v@example.com', name: 'Віталій', pictureUrl: null };

function stubFetch(status: number) {
  const fetch = vi.fn((request: Request) => {
    void request;
    return Promise.resolve(
      new Response(JSON.stringify(status === 200 ? member : {}), {
        status,
        headers: { 'Content-Type': 'application/json' },
      }),
    );
  });
  vi.stubGlobal('fetch', fetch);
  return fetch;
}

beforeEach(() => localStorage.clear());
afterEach(() => vi.unstubAllGlobals());

test('sends the stored token as a bearer header', async () => {
  session.save('token-1');
  const fetch = stubFetch(200);

  await api.GET('/api/identity/me');

  expect(fetch.mock.calls[0]?.[0].headers.get('Authorization')).toBe(
    'Bearer token-1',
  );
});

test('sends no Authorization header without a session', async () => {
  session.clear('signed-out');
  const fetch = stubFetch(200);

  await api.GET('/api/identity/me');

  expect(fetch.mock.calls[0]?.[0].headers.has('Authorization')).toBe(false);
});

test('a 401 to the stored token ends the session as expired', async () => {
  session.save('token-1');
  stubFetch(401);

  await api.GET('/api/identity/me');

  expect(session.current()).toEqual({ token: null, ended: 'expired' });
});

test('a 401 without a token (a refused sign-in) changes nothing', async () => {
  session.clear('signed-out');
  stubFetch(401);

  await api.POST('/api/identity/sign-in', { body: { credential: 'bad' } });

  expect(session.current()).toEqual({ token: null, ended: 'signed-out' });
});

test('a late 401 to an older token keeps the newer session', async () => {
  session.save('old');
  vi.stubGlobal(
    'fetch',
    vi.fn(() => {
      session.save('new');
      return Promise.resolve(new Response('{}', { status: 401 }));
    }),
  );

  await api.GET('/api/identity/me');

  expect(session.token()).toBe('new');
});
