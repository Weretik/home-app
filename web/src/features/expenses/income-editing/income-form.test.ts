import {
  incomeFormValues,
  toIncomeRequest,
  validateIncome,
} from './income-form';

const month = { year: 2026, month: 10 };

test('a new income is for Віталій on a day of the open month', () => {
  const values = incomeFormValues(undefined, month);
  expect(values).toMatchObject({ person: 'Vitaliy', amount: '', comment: '' });
  expect(values.date.startsWith('2026-10-')).toBe(true);
});

test('an existing income fills the form', () => {
  expect(
    incomeFormValues(
      {
        id: 'i1',
        version: 2,
        date: '2026-10-05',
        person: 'Maria',
        amount: 30000.5,
        comment: null,
      },
      month,
    ),
  ).toEqual({
    date: '2026-10-05',
    person: 'Maria',
    amount: '30000,50',
    comment: '',
  });
});

test('checks the date and the amount', () => {
  const errors = validateIncome({
    date: '',
    person: 'Vitaliy',
    amount: '0',
    comment: '',
  });
  expect(errors.date).toBeTruthy();
  expect(errors.amount).toBeTruthy();
  expect(errors.comment).toBeUndefined();
});

test('turns the form into the request: a number, and no comment when empty', () => {
  expect(
    toIncomeRequest({
      date: '2026-10-05',
      person: 'Maria',
      amount: '1 250,5',
      comment: '  ',
    }),
  ).toEqual({
    date: '2026-10-05',
    person: 'Maria',
    amount: 1250.5,
    comment: null,
  });
});
