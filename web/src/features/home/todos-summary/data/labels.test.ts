import { expect, test } from 'vitest';
import { formatDue } from './labels';

test('formats a due date against the summary’s today', () => {
  expect(formatDue('2026-11-26', '2026-11-26')).toBe('сегодня');
  expect(formatDue('2026-11-27', '2026-11-26')).toBe('завтра');
  expect(formatDue('2026-11-25', '2026-11-26')).toBe('вчера');
  expect(formatDue('2026-11-30', '2026-11-26')).toBe('пн 30.11');
  expect(formatDue('2027-01-04', '2026-11-26')).toBe('пн 04.01.2027');
});
