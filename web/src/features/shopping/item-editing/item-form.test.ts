import type { ShoppingItem } from '../data/shopping-api';
import { checkItem, itemRequest, itemValues } from './item-form';

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

test('an item opens with its values, empty quantity and note as empty text', () => {
  expect(itemValues(item({ quantity: '2 л', urgent: true }))).toEqual({
    name: 'Молоко',
    quantity: '2 л',
    note: '',
    departmentId: 'd1',
    urgent: true,
  });
});

test('checks the name, the lengths and the department', () => {
  const values = itemValues(item({}));

  expect(checkItem(values)).toEqual({});
  expect(checkItem({ ...values, name: '  ', departmentId: '' })).toEqual({
    name: 'Введіть назву',
    departmentId: 'Виберіть відділ',
  });
  expect(
    checkItem({
      ...values,
      name: 'я'.repeat(81),
      quantity: '1'.repeat(31),
      note: 'н'.repeat(201),
    }),
  ).toEqual({
    name: 'Не довше 80 символів',
    quantity: 'Не довше 30 символів',
    note: 'Не довше 200 символів',
  });
});

test('sends trimmed text and null for an empty quantity or note', () => {
  expect(
    itemRequest({
      name: ' Сыр ',
      quantity: ' ',
      note: ' к пасте ',
      departmentId: 'd1',
      urgent: false,
    }),
  ).toEqual({
    name: 'Сыр',
    quantity: null,
    note: 'к пасте',
    departmentId: 'd1',
    urgent: false,
  });
});
