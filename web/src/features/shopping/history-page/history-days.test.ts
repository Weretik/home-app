import type { ShoppingItem } from '../data/shopping-api';
import { dayLabel, groupByDay } from './history-days';

const item = (over: Partial<ShoppingItem>): ShoppingItem =>
  ({
    id: 'i1',
    name: 'Молоко',
    quantity: null,
    note: null,
    departmentId: 'd1',
    departmentName: 'Молочка',
    departmentIcon: '🥛',
    departmentSortOrder: 1,
    urgent: false,
    status: 'Needed',
    addedBy: 'Vitaliy',
    addedAtUtc: '2026-11-26T08:00:00Z',
    boughtBy: null,
    boughtAtUtc: null,
    version: 1,
    ...over,
  }) as ShoppingItem;

const now = new Date(2026, 10, 26, 12, 0);
const at = (day: number, hour: number) =>
  new Date(2026, 10, day, hour, 0).toISOString();

test('groups rows by local day, newest day first, today and yesterday named', () => {
  const days = groupByDay(
    [
      item({ id: 'a', status: 'Bought', boughtAtUtc: at(26, 11) }),
      item({ id: 'b', status: 'Bought', boughtAtUtc: at(26, 9) }),
      item({ id: 'c', status: 'Bought', boughtAtUtc: at(25, 20) }),
      item({ id: 'd', status: 'Bought', boughtAtUtc: at(20, 8) }),
    ],
    now,
  );

  expect(days.map((d) => d.label)).toEqual([
    'Сегодня',
    'Вчера',
    'пятница, 20 ноября',
  ]);
  expect(days[0]!.items.map((i) => i.id)).toEqual(['a', 'b']);
});

test('another year is written out', () => {
  expect(dayLabel(new Date(2025, 0, 3), now)).toBe('пятница, 3 января 2025');
});
