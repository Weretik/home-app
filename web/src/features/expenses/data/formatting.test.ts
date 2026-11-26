import { formatAmount, formatDay, formatRecordCount } from './formatting';

// Intl separates groups and the symbol with non-breaking spaces.
const plain = (text: string) => text.replace(/\s/g, ' ');

test('formats hryvnias with two decimals', () => {
  expect(plain(formatAmount(95024))).toBe('95 024,00 ₴');
  expect(plain(formatAmount(1234.5))).toBe('1 234,50 ₴');
  expect(plain(formatAmount(0))).toBe('0,00 ₴');
});

test('formats a contract date as a short local day', () => {
  expect(formatDay('2026-10-01')).toBe('чт, 1 окт.');
});

test('counts records with Russian plural forms', () => {
  expect(formatRecordCount(1)).toBe('1 запись');
  expect(formatRecordCount(3)).toBe('3 записи');
  expect(formatRecordCount(12)).toBe('12 записей');
  expect(formatRecordCount(21)).toBe('21 запись');
});
