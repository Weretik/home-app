import type { ShoppingItem } from '../data/shopping-api';
import { cartItems, countItems, groupNeeded } from './list-groups';

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

test('groups needed items by department in store order, urgent first, then oldest added', () => {
  const groups = groupNeeded([
    item({
      id: 'bread',
      departmentId: 'd2',
      departmentName: 'Хлеб',
      departmentSortOrder: 2,
      addedAtUtc: '2026-11-26T07:00:00Z',
    }),
    item({ id: 'milk', addedAtUtc: '2026-11-26T09:00:00Z' }),
    item({ id: 'kefir', addedAtUtc: '2026-11-26T08:00:00Z' }),
    item({ id: 'cheese', urgent: true, addedAtUtc: '2026-11-26T10:00:00Z' }),
    item({
      id: 'bought',
      status: 'Bought',
      boughtAtUtc: '2026-11-26T09:30:00Z',
    }),
  ]);

  expect(groups.map((g) => g.name)).toEqual(['Молочка', 'Хлеб']);
  expect(groups[0]!.items.map((i) => i.id)).toEqual([
    'cheese',
    'kefir',
    'milk',
  ]);
  expect(groups[1]!.items.map((i) => i.id)).toEqual(['bread']);
});

test('the cart holds bought items, latest first, and the counts add up', () => {
  const items = [
    item({ id: 'a', status: 'Bought', boughtAtUtc: '2026-11-26T08:00:00Z' }),
    item({ id: 'b', status: 'Bought', boughtAtUtc: '2026-11-26T09:00:00Z' }),
    item({ id: 'c', urgent: true }),
    item({ id: 'd' }),
  ];

  expect(cartItems(items).map((i) => i.id)).toEqual(['b', 'a']);
  expect(countItems(items)).toEqual({ needed: 2, urgent: 1, inCart: 2 });
});
