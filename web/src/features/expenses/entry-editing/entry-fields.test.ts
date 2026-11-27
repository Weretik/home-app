import {
  amountError,
  amountText,
  commentError,
  dateError,
  defaultDateIn,
  defaultEntryDate,
  parseAmount,
} from './entry-fields';

test('starts a new entry today in the current month, else on the 1st', () => {
  const today = new Date(2026, 10, 26);
  expect(defaultEntryDate({ year: 2026, month: 11 }, today)).toBe('2026-11-26');
  expect(defaultEntryDate({ year: 2026, month: 9 }, today)).toBe('2026-09-01');
});

test('starts a new entry today within a period, else on its first day', () => {
  const today = new Date(2026, 10, 26);
  const period = { from: '2026-11-20', to: '2026-12-05' };
  expect(defaultDateIn(period, today)).toBe('2026-11-26');
  expect(defaultDateIn({ from: '2026-10-01', to: '2026-10-31' }, today)).toBe(
    '2026-10-01',
  );
});

test('reads amounts the way people type them', () => {
  expect(parseAmount('1 234,5')).toBe(1234.5);
  expect(parseAmount('99.99')).toBe(99.99);
  expect(parseAmount('1,234')).toBeUndefined();
  expect(parseAmount('-5')).toBeUndefined();
  expect(parseAmount('abc')).toBeUndefined();
  expect(amountText(1234.5)).toBe('1234,50');
  expect(amountText(-700)).toBe('700');
});

test('checks amount, date and comment', () => {
  expect(amountError('')).toBe('Введіть суму');
  expect(amountError('0')).toBe('Сума має бути більшою за нуль');
  expect(amountError('1,234')).toBeDefined();
  expect(amountError('20000000')).toBe('Завелика сума');
  expect(amountError('15,50')).toBeUndefined();
  expect(dateError('')).toBe('Виберіть дату');
  expect(dateError('1999-12-31')).toBeDefined();
  expect(dateError('2026-11-26')).toBeUndefined();
  expect(commentError('x'.repeat(501))).toBeDefined();
  expect(commentError('x'.repeat(500))).toBeUndefined();
});
