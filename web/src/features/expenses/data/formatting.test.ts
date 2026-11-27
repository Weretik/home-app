import {
  formatAmount,
  formatDay,
  formatRecordCount,
  formatSignedAmount,
} from './formatting';

// Intl separates groups and the symbol with non-breaking spaces.
const plain = (text: string) => text.replace(/\s/g, ' ');

test('formats hryvnias with two decimals', () => {
  expect(plain(formatAmount(95024))).toBe('95 024,00 ₴');
  expect(plain(formatAmount(1234.5))).toBe('1 234,50 ₴');
  expect(plain(formatAmount(0))).toBe('0,00 ₴');
});

test('formats a contract date as a short local day', () => {
  expect(formatDay('2026-10-01')).toBe('чт, 1 жовт.');
});

test('counts records with Ukrainian plural forms', () => {
  expect(formatRecordCount(1)).toBe('1 запис');
  expect(formatRecordCount(3)).toBe('3 записи');
  expect(formatRecordCount(12)).toBe('12 записів');
  expect(formatRecordCount(21)).toBe('21 запис');
});

test('writes the sign of a signed amount', () => {
  expect(plain(formatSignedAmount(1500))).toBe('+1 500,00 ₴');
  expect(plain(formatSignedAmount(-700))).toBe('\u2212700,00 ₴');
  expect(plain(formatSignedAmount(0))).toBe('0,00 ₴');
});
