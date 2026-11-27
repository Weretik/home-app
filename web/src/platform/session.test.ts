import { afterEach, beforeEach, vi } from 'vitest';
import { SESSION_STORAGE_KEY, session } from './session';

beforeEach(() => localStorage.clear());
afterEach(() => vi.restoreAllMocks());

test('starts signed out without a stored token', () => {
  expect(session.current()).toEqual({ token: null, ended: null });
});

test('reads a stored token synchronously', () => {
  localStorage.setItem(SESSION_STORAGE_KEY, 'stored');

  expect(session.token()).toBe('stored');
});

test('saves the token and tells the listeners', () => {
  const listener = vi.fn();
  const unsubscribe = session.subscribe(listener);

  session.save('token-1');

  expect(localStorage.getItem(SESSION_STORAGE_KEY)).toBe('token-1');
  expect(session.current()).toEqual({ token: 'token-1', ended: null });
  expect(listener).toHaveBeenCalledTimes(1);
  unsubscribe();
});

test('keeps the same snapshot until the token changes', () => {
  session.save('token-1');

  expect(session.current()).toBe(session.current());
});

test.each(['expired', 'signed-out'] as const)(
  'clears the token with the reason %s',
  (reason) => {
    session.save('token-1');

    session.clear(reason);

    expect(localStorage.getItem(SESSION_STORAGE_KEY)).toBeNull();
    expect(session.current()).toEqual({ token: null, ended: reason });
  },
);

test('a sign-in again forgets why the last session ended', () => {
  session.clear('expired');

  session.save('token-2');

  expect(session.current().ended).toBeNull();
});

test('follows a sign-in and a sign-out in another tab (storage event)', () => {
  session.clear('signed-out');
  const listener = vi.fn();
  const unsubscribe = session.subscribe(listener);

  localStorage.setItem(SESSION_STORAGE_KEY, 'other-tab');
  window.dispatchEvent(
    new StorageEvent('storage', { key: SESSION_STORAGE_KEY }),
  );
  expect(session.token()).toBe('other-tab');

  localStorage.removeItem(SESSION_STORAGE_KEY);
  window.dispatchEvent(
    new StorageEvent('storage', { key: SESSION_STORAGE_KEY }),
  );
  expect(session.current()).toEqual({ token: null, ended: 'signed-out' });
  expect(listener).toHaveBeenCalledTimes(2);

  window.dispatchEvent(new StorageEvent('storage', { key: 'mui-mode' }));
  expect(listener).toHaveBeenCalledTimes(2);
  unsubscribe();
});

test('keeps the token in memory when storage is blocked', () => {
  vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
    throw new DOMException('blocked', 'SecurityError');
  });
  vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
    throw new DOMException('blocked', 'SecurityError');
  });

  session.save('in-memory');

  expect(session.token()).toBe('in-memory');
});
