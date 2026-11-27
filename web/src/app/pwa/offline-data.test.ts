import { QueryClient } from '@tanstack/react-query';
import { vi } from 'vitest';
import { offlineLoads, syncOfflineData } from './offline-data';

const now = new Date('2026-11-27T10:00:00Z');
const keys = (person?: 'Vitaliy' | 'Maria') =>
  offlineLoads(person, now).map(({ key }) => JSON.stringify(key));

test('keeps every main page: lists, home summaries, the current month and the settings', () => {
  const all = keys('Vitaliy');
  for (const expected of [
    ['shopping', 'list'],
    ['shopping', 'summary'],
    ['todos', 'summary'],
    ['gifts', 'summary'],
    ['expenses', 'summary', { year: 2026, month: 11 }],
    ['expenses', 'budget'],
    ['expenses', 'planned', { year: 2026, month: 11 }],
    ['expenses', 'list', { year: 2026, month: 11 }, {}],
    ['expenses', 'incomes', { year: 2026, month: 11 }, {}],
    ['gifts', 'items', { archived: false }],
    ['gifts', 'items', { archived: true }],
    ['notes', 'categories'],
    ['todos', 'categories'],
  ])
    expect(all).toContain(JSON.stringify(expected));
});

test('the default lists follow the signed-in person: their to-dos, all to-dos, and each «Про нас» tab', () => {
  const vitaliy = keys('Vitaliy');
  expect(
    vitaliy.some((key) => key.includes('"assignee":["Vitaliy","Shared"]')),
  ).toBe(true);
  expect(vitaliy).toContain(
    JSON.stringify(['notes', 'items', { about: 'Vitaliy' }]),
  );
  expect(vitaliy).toContain(
    JSON.stringify(['notes', 'items', { about: 'Maria' }]),
  );
  expect(vitaliy).toContain(JSON.stringify(['notes', 'items', {}]));
});

test('loads each query through the client, so a fresh one is not asked again', async () => {
  const client = new QueryClient();
  const single = vi.spyOn(client, 'prefetchQuery').mockResolvedValue();
  const paged = vi.spyOn(client, 'prefetchInfiniteQuery').mockResolvedValue();

  syncOfflineData(client, 'Maria');

  expect(single).toHaveBeenCalled();
  expect(paged).toHaveBeenCalledTimes(7);
  expect(single.mock.calls[0]?.[0]).toMatchObject({ staleTime: 60_000 });
});
