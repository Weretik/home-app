import type { ShoppingItem } from '../data/shopping-api';
import {
  dayLabel,
  groupByDay,
  groupByMonth,
  monthHeading,
  monthLabel,
  purchaseCount,
} from './history-groups';

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

const on = (year: number, month: number, day: number) =>
  new Date(year, month - 1, day, 10, 0).toISOString();

test('groups days under their month, newest month first, with counts', () => {
  const months = groupByMonth(
    [
      item({ id: 'a', status: 'Bought', boughtAtUtc: on(2026, 11, 26) }),
      item({ id: 'b', status: 'Bought', boughtAtUtc: on(2026, 11, 2) }),
      item({ id: 'c', status: 'Bought', boughtAtUtc: on(2026, 10, 31) }),
      item({ id: 'd', status: 'Bought', boughtAtUtc: on(2025, 12, 31) }),
    ],
    now,
  );

  expect(months.map((m) => m.key)).toEqual(['2026-11', '2026-10', '2025-12']);
  expect(months.map(monthHeading)).toEqual([
    'Ноябрь 2026 · 2 покупки',
    'Октябрь 2026 · 1 покупка',
    'Декабрь 2025 · 1 покупка',
  ]);
  expect(months[0]!.days.map((d) => d.label)).toEqual([
    'Сегодня',
    'понедельник, 2 ноября',
  ]);
});

test('the last month may continue on the next page; rows without a time come last', () => {
  const months = groupByMonth(
    [
      item({ id: 'a', status: 'Bought', boughtAtUtc: on(2026, 11, 26) }),
      item({ id: 'b', status: 'Bought', boughtAtUtc: on(2026, 10, 3) }),
      item({ id: 'c', status: 'Bought', boughtAtUtc: null }),
    ],
    now,
    true,
  );

  expect(months.map((m) => [m.label, m.partial])).toEqual([
    ['Ноябрь 2026', false],
    ['Октябрь 2026', true],
    ['Без даты', false],
  ]);
  expect(monthHeading(months[1]!)).toBe('Октябрь 2026 · 1+ покупок');
  expect(groupByMonth([], now, true)).toEqual([]);
});

test('counts purchases in Russian plural', () => {
  expect(purchaseCount(1)).toBe('1 покупка');
  expect(purchaseCount(4)).toBe('4 покупки');
  expect(purchaseCount(11)).toBe('11 покупок');
  expect(purchaseCount(21)).toBe('21 покупка');
  expect(purchaseCount(50, true)).toBe('50+ покупок');
  expect(monthLabel('2026-05-01')).toBe('Май 2026');
});
