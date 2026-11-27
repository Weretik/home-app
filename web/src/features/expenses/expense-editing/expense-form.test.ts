import { expenseFormValues, toExpenseRequest } from './expense-form';

test('a new expense counts in the limits; «Не в ліміти» is kept and sent', () => {
  const values = expenseFormValues(undefined, '2026-11-26');
  expect(values.outsideLimits).toBe(false);
  expect(
    toExpenseRequest({
      ...values,
      categoryId: 'c-gifts',
      amount: '700',
      outsideLimits: true,
    }),
  ).toEqual({
    date: '2026-11-26',
    payer: 'Shared',
    categoryId: 'c-gifts',
    amount: 700,
    paymentMethod: 'Card',
    comment: null,
    outsideLimits: true,
  });
});
