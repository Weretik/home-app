import { afterEach, vi } from 'vitest';
import {
  GOOGLE_SCRIPT_URL,
  loadGoogleIdentity,
  showGoogleButton,
  type GoogleAccountsId,
} from './google-identity';

const fakeId = (): GoogleAccountsId => ({
  initialize: vi.fn(),
  renderButton: vi.fn(),
  prompt: vi.fn(),
  disableAutoSelect: vi.fn(),
});

const scripts = () =>
  document.head.querySelectorAll(`script[src="${GOOGLE_SCRIPT_URL}"]`);

afterEach(() => {
  delete window.google;
  scripts().forEach((script) => script.remove());
});

test('uses Google when the script is already there', async () => {
  const id = fakeId();
  window.google = { accounts: { id } };

  await expect(loadGoogleIdentity()).resolves.toBe(id);
  expect(scripts()).toHaveLength(0);
});

test('adds the script once and resolves when it loads', async () => {
  const first = loadGoogleIdentity();
  const second = loadGoogleIdentity();
  const id = fakeId();

  expect(scripts()).toHaveLength(1);
  window.google = { accounts: { id } };
  scripts()[0]?.dispatchEvent(new Event('load'));

  await expect(first).resolves.toBe(id);
  await expect(second).resolves.toBe(id);
});

test('a failed load can be tried again', async () => {
  const failed = loadGoogleIdentity();
  scripts()[0]?.dispatchEvent(new Event('error'));
  await expect(failed).rejects.toThrow();
  expect(scripts()).toHaveLength(0);

  void loadGoogleIdentity().catch(() => {});
  expect(scripts()).toHaveLength(1);
  scripts()[0]?.dispatchEvent(new Event('error'));
});

test('draws the button in the colour scheme and passes the credential on', () => {
  const id = fakeId();
  const host = document.createElement('div');
  const onCredential = vi.fn();

  showGoogleButton(id, host, {
    clientId: 'client-1',
    scheme: 'dark',
    width: 120,
    autoSelect: true,
    onCredential,
  });

  const config = vi.mocked(id.initialize).mock.calls[0]?.[0];
  expect(config).toMatchObject({ client_id: 'client-1', auto_select: true });
  config?.callback({ credential: 'google-token' });
  expect(onCredential).toHaveBeenCalledWith('google-token');
  expect(id.renderButton).toHaveBeenCalledWith(
    host,
    expect.objectContaining({
      theme: 'filled_black',
      locale: 'uk',
      width: 200,
    }),
  );
  expect(id.prompt).toHaveBeenCalled();
  expect(id.disableAutoSelect).not.toHaveBeenCalled();
});

test('after a sign-out stops the automatic sign-in and One Tap', () => {
  const id = fakeId();

  showGoogleButton(id, document.createElement('div'), {
    clientId: 'client-1',
    scheme: 'light',
    width: 300,
    autoSelect: false,
    onCredential: vi.fn(),
  });

  expect(id.disableAutoSelect).toHaveBeenCalled();
  expect(vi.mocked(id.initialize).mock.calls[0]?.[0]).toMatchObject({
    auto_select: false,
  });
  expect(id.renderButton).toHaveBeenCalledWith(
    expect.anything(),
    expect.objectContaining({ theme: 'outline', width: 300 }),
  );
  expect(id.prompt).not.toHaveBeenCalled();
});

test('asks Google for the Ukrainian button and One Tap', () => {
  expect(new URL(GOOGLE_SCRIPT_URL).searchParams.get('hl')).toBe('uk');
});
