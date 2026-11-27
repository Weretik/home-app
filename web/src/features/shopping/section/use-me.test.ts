import { act, renderHook } from '@testing-library/react';
import { afterEach, vi } from 'vitest';
import { meStorageKey, readMe, useMe } from './use-me';

afterEach(() => {
  vi.restoreAllMocks();
  localStorage.clear();
});

test('defaults to Віталій and remembers the choice on the device', () => {
  const { result } = renderHook(() => useMe());
  expect(result.current[0]).toBe('Vitaliy');

  act(() => result.current[1]('Maria'));

  expect(result.current[0]).toBe('Maria');
  expect(localStorage.getItem(meStorageKey)).toBe('Maria');
  expect(readMe()).toBe('Maria');
});

test('falls back to the default when storage holds junk or throws', () => {
  localStorage.setItem(meStorageKey, 'Someone');
  expect(readMe()).toBe('Vitaliy');

  vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
    throw new Error('blocked');
  });
  expect(readMe()).toBe('Vitaliy');
});
