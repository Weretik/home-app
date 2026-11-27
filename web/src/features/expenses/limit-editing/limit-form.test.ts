import { limitFormValues, toLimitRequest, validateLimit } from './limit-form';

test('starts a new limit monthly and empty, an existing one from its values', () => {
  expect(limitFormValues()).toEqual({
    name: '',
    period: 'Month',
    amount: '',
    categoryIds: [],
  });
  expect(
    limitFormValues({
      id: 'l1',
      version: 2,
      name: 'Еда и быт',
      period: 'Quarter',
      amount: 15000,
      categoryIds: ['a', 'b'],
    }),
  ).toEqual({
    name: 'Еда и быт',
    period: 'Quarter',
    amount: '15000',
    categoryIds: ['a', 'b'],
  });
});

test('checks name, amount and categories', () => {
  const errors = validateLimit(limitFormValues());
  expect(errors.name).toBe('Введіть назву');
  expect(errors.amount).toBe('Введіть суму');
  expect(errors.categoryIds).toBe('Виберіть хоча б одну категорію');
  expect(
    validateLimit({
      name: 'x'.repeat(101),
      period: 'Month',
      amount: '1',
      categoryIds: ['a'],
    }).name,
  ).toBe('Не довше 100 символів');
});

test('sends a trimmed name, the amount as a number and distinct categories', () => {
  expect(
    toLimitRequest({
      name: '  Кафе ',
      period: 'Month',
      amount: '3 000,5',
      categoryIds: ['a', 'a', 'b'],
    }),
  ).toEqual({
    name: 'Кафе',
    period: 'Month',
    amount: 3000.5,
    categoryIds: ['a', 'b'],
  });
});
