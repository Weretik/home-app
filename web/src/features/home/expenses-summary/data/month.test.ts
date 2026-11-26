import { currentMonth, formatMonth, parseMonth, shiftMonth } from './month';

// The same cases as expenses/data/month.test.ts: the summary keeps its own copy (features never import each other).
test('takes the current month from the date', () => {
  expect(currentMonth(new Date(2026, 10, 26))).toEqual({
    year: 2026,
    month: 11,
  });
});

test('shifts across year boundaries', () => {
  expect(shiftMonth({ year: 2026, month: 1 }, -1)).toEqual({
    year: 2025,
    month: 12,
  });
  expect(shiftMonth({ year: 2026, month: 12 }, 1)).toEqual({
    year: 2027,
    month: 1,
  });
});

test('reads the month from the URL, or falls back', () => {
  const fallback = { year: 2026, month: 11 };
  expect(parseMonth('2026', '10', fallback)).toEqual({ year: 2026, month: 10 });
  expect(parseMonth('2026', '13', fallback)).toBe(fallback);
  expect(parseMonth(null, '5', fallback)).toBe(fallback);
  expect(parseMonth('1999', '5', fallback)).toBe(fallback);
});

test('names the month in Russian with a capital letter', () => {
  expect(formatMonth({ year: 2026, month: 10 })).toBe('Октябрь 2026');
});
