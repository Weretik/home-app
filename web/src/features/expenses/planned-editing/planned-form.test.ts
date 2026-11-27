import {
  plannedFormValues,
  toPlannedRequest,
  validatePlanned,
} from './planned-form';

const november = { year: 2026, month: 11 };

test('starts a new plan shared and empty in the open month, an existing one from its values', () => {
  expect(plannedFormValues(undefined, november)).toEqual({
    year: 2026,
    month: 11,
    name: '',
    categoryId: '',
    payer: 'Shared',
    amount: '',
  });
  expect(
    plannedFormValues(
      {
        id: 'p1',
        version: 3,
        year: 2026,
        month: 12,
        name: 'Страховка',
        categoryId: 'c-car',
        categoryName: 'Машина',
        payer: 'Vitaliy',
        amount: 4200.5,
      },
      november,
    ),
  ).toEqual({
    year: 2026,
    month: 12,
    name: 'Страховка',
    categoryId: 'c-car',
    payer: 'Vitaliy',
    amount: '4200,50',
  });
});

test('checks name, category and amount', () => {
  const errors = validatePlanned(plannedFormValues(undefined, november));
  expect(errors.name).toBe('Введіть назву');
  expect(errors.categoryId).toBe('Виберіть категорію');
  expect(errors.amount).toBe('Введіть суму');
  const filled = {
    year: 2026,
    month: 11,
    name: '   ',
    categoryId: 'c',
    payer: 'Shared' as const,
    amount: '0',
  };
  expect(validatePlanned(filled).name).toBe('Введіть назву');
  expect(validatePlanned(filled).amount).toBe('Сума має бути більшою за нуль');
  expect(validatePlanned({ ...filled, name: 'x'.repeat(101) }).name).toBe(
    'Не довше 100 символів',
  );
  expect(
    Object.values(
      validatePlanned({ ...filled, name: 'x'.repeat(100), amount: '1' }),
    ).every((message) => !message),
  ).toBe(true);
});

test('sends a trimmed name and the amount as a number', () => {
  expect(
    toPlannedRequest({
      year: 2026,
      month: 11,
      name: '  Подарок маме ',
      categoryId: 'c-gifts',
      payer: 'Maria',
      amount: '1 500,5',
    }),
  ).toEqual({
    year: 2026,
    month: 11,
    name: 'Подарок маме',
    categoryId: 'c-gifts',
    payer: 'Maria',
    amount: 1500.5,
  });
});
