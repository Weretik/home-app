import {
  limitFormValues,
  sendsAmountFrom,
  toLimitRequest,
  validateLimit,
} from './limit-form';

const now = new Date('2026-11-30T22:30:00Z'); // already December in Kyiv

const cafe = {
  id: 'l1',
  version: 2,
  name: 'Еда и быт',
  period: 'Quarter' as const,
  amount: 15000,
  amounts: [{ from: '2026-01-01', amount: 15000 }],
  categoryIds: ['a', 'b'],
};

test('starts a new limit monthly and empty, with no «Діє з» month', () => {
  expect(limitFormValues(undefined, now)).toEqual({
    name: '',
    period: 'Month',
    amount: '',
    categoryIds: [],
  });
  expect(limitFormValues(undefined, now)).not.toHaveProperty('amountFrom');
});

test('starts an existing limit from its values and the current Kyiv month', () => {
  expect(limitFormValues(cafe, now)).toEqual({
    name: 'Еда и быт',
    period: 'Quarter',
    amount: '15000',
    categoryIds: ['a', 'b'],
    amountFrom: '2026-12',
  });
});

test('checks name, amount and categories', () => {
  const errors = validateLimit(limitFormValues());
  expect(errors.name).toBe('Введіть назву');
  expect(errors.amount).toBe('Введіть суму');
  expect(errors.categoryIds).toBe('Виберіть хоча б одну категорію');
  expect(errors.amountFrom).toBeUndefined();
  expect(
    validateLimit({
      name: 'x'.repeat(101),
      period: 'Month',
      amount: '1',
      categoryIds: ['a'],
    }).name,
  ).toBe('Не довше 100 символів');
});

test('an edited limit needs its month', () => {
  const values = limitFormValues(cafe, now);
  expect(validateLimit(values).amountFrom).toBeUndefined();
  expect(validateLimit({ ...values, amountFrom: '' }).amountFrom).toBe(
    'Виберіть місяць',
  );
  expect(validateLimit({ ...values, amountFrom: '2026-13' }).amountFrom).toBe(
    'Виберіть місяць',
  );
});

test('sends a trimmed name, the amount as a number and distinct categories', () => {
  const request = toLimitRequest({
    name: '  Кафе ',
    period: 'Month',
    amount: '3 000,5',
    categoryIds: ['a', 'a', 'b'],
  });
  expect(request).toEqual({
    name: 'Кафе',
    period: 'Month',
    amount: 3000.5,
    categoryIds: ['a', 'b'],
  });
  expect(request).not.toHaveProperty('amountFrom');
});

test('sends an edit with the first day of its month', () => {
  const initial = limitFormValues(cafe, now);
  expect(
    toLimitRequest({ ...initial, amountFrom: '2027-02' }, initial),
  ).toEqual({
    name: 'Еда и быт',
    period: 'Quarter',
    amount: 15000,
    categoryIds: ['a', 'b'],
    amountFrom: '2027-02-01',
  });
});

test('a changed amount is sent from the default month', () => {
  const initial = limitFormValues(cafe, now);
  expect(
    toLimitRequest({ ...initial, amount: '12 000' }, initial),
  ).toMatchObject({ amount: 12000, amountFrom: '2026-12-01' });
});

test('a rename or new categories with the amount and month untouched send no «Діє з»', () => {
  const initial = limitFormValues(cafe, now);
  const renamed = { ...initial, name: 'Їжа', categoryIds: ['a'] };
  expect(sendsAmountFrom(renamed, initial)).toBe(false);
  expect(toLimitRequest(renamed, initial)).not.toHaveProperty('amountFrom');
  // the same amount typed differently is still the same amount
  expect(sendsAmountFrom({ ...initial, amount: '15 000,00' }, initial)).toBe(
    false,
  );
});

test('the same amount from another month is sent: it replaces the later steps', () => {
  const initial = limitFormValues(cafe, now);
  expect(sendsAmountFrom({ ...initial, amountFrom: '2027-04' }, initial)).toBe(
    true,
  );
});

test('a new limit never sends «Діє з»', () => {
  const initial = limitFormValues(undefined, now);
  expect(sendsAmountFrom({ ...initial, amount: '100' }, initial)).toBe(false);
});
