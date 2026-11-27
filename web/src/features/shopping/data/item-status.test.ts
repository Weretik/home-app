import { withStatus, withoutCart } from './item-status';
import type { ShoppingItem, ShoppingList } from './shopping-api';

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

const list = (...items: ShoppingItem[]): ShoppingList => ({ items });
const now = new Date('2026-11-26T10:00:00Z');

test('a ticked item becomes bought by me now', () => {
  const after = withStatus(
    list(item({}), item({ id: 'i2' })),
    { id: 'i1', status: 'Bought', by: 'Maria' },
    now,
  );

  expect(after.items[0]).toMatchObject({
    status: 'Bought',
    boughtBy: 'Maria',
    boughtAtUtc: '2026-11-26T10:00:00.000Z',
  });
  expect(after.items[1]!.status).toBe('Needed');
});

test('an unticked item is needed again without buyer and time', () => {
  const after = withStatus(
    list(
      item({
        status: 'Bought',
        boughtBy: 'Maria',
        boughtAtUtc: '2026-11-26T09:00:00Z',
      }),
    ),
    { id: 'i1', status: 'Needed', by: 'Vitaliy' },
    now,
  );

  expect(after.items[0]).toMatchObject({
    status: 'Needed',
    boughtBy: null,
    boughtAtUtc: null,
  });
});

test('an unknown id or the same status leaves the list as it is', () => {
  const before = list(item({}));

  expect(
    withStatus(before, { id: 'x', status: 'Bought', by: 'Maria' }).items[0],
  ).toBe(before.items[0]);
  expect(
    withStatus(before, { id: 'i1', status: 'Needed', by: 'Maria' }).items[0],
  ).toBe(before.items[0]);
});

test('clearing the cart drops the bought items and keeps the needed ones', () => {
  const after = withoutCart(
    list(item({}), item({ id: 'i2', status: 'Bought', boughtBy: 'Maria' })),
  );

  expect(after.items.map((row) => row.id)).toEqual(['i1']);
});
