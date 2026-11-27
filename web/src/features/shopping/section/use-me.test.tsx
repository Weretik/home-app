import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { act, renderHook } from '@testing-library/react';
import type { ReactNode } from 'react';
import { afterEach, vi } from 'vitest';
import { memberKeys, type FamilyMember } from '../../../platform/member-api';
import { meStorageKey, readMe, useMe } from './use-me';

afterEach(() => {
  vi.restoreAllMocks();
  sessionStorage.clear();
});

const masha: FamilyMember = {
  email: 'masha@family.test',
  name: 'Маша',
  pictureUrl: null,
  person: 'Maria',
};

function renderMe(member?: FamilyMember) {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false, enabled: false } },
  });
  if (member) client.setQueryData(memberKeys.me, member);
  const wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={client}>{children}</QueryClientProvider>
  );
  return renderHook(() => useMe(), { wrapper });
}

test('starts on the signed-in person', () => {
  const { result } = renderMe(masha);

  expect(result.current[0]).toBe('Maria');
});

test('a hand-picked person wins for this tab only', () => {
  const { result } = renderMe(masha);

  act(() => result.current[1]('Vitaliy'));

  expect(result.current[0]).toBe('Vitaliy');
  expect(sessionStorage.getItem(meStorageKey(masha.email))).toBe('Vitaliy');
  expect(readMe(masha.email)).toBe('Vitaliy');
  expect(readMe('vitaliy@family.test')).toBeNull();
});

test('falls back to Віталій while nobody is known', () => {
  const { result } = renderMe();

  expect(result.current[0]).toBe('Vitaliy');
  expect(
    renderMe({ ...masha, email: 'guest@family.test', person: null }).result
      .current[0],
  ).toBe('Vitaliy');
});

test('ignores junk in storage and survives blocked storage', () => {
  sessionStorage.setItem(meStorageKey(masha.email), 'Someone');
  expect(readMe(masha.email)).toBeNull();

  vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
    throw new Error('blocked');
  });
  vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
    throw new Error('blocked');
  });
  expect(readMe(masha.email)).toBeNull();
  const { result } = renderMe(masha);
  act(() => result.current[1]('Vitaliy'));
  expect(result.current[0]).toBe('Vitaliy');
});
