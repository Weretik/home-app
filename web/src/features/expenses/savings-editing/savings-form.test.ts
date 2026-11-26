import { savingsFormValues, toSavingsRequest } from './savings-form';

const base = {
  date: '2026-10-05',
  person: 'Vitaliy' as const,
  amount: '1500,5',
  comment: ' ',
};

test('signs the amount by the direction', () => {
  expect(toSavingsRequest({ ...base, direction: 'Deposit' })).toEqual({
    date: '2026-10-05',
    person: 'Vitaliy',
    amount: 1500.5,
    comment: null,
  });
  expect(toSavingsRequest({ ...base, direction: 'Withdrawal' }).amount).toBe(
    -1500.5,
  );
});

test('edits a withdrawal as a positive amount taken out', () => {
  const values = savingsFormValues(
    {
      id: 'm1',
      version: 3,
      date: '2026-10-05',
      person: 'Maria',
      amount: -700,
      direction: 'Withdrawal',
      comment: null,
    },
    { year: 2026, month: 10 },
  );
  expect(values.direction).toBe('Withdrawal');
  expect(values.amount).toBe('700');
});
